// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure;
using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using global::Azure.ResourceManager.Synapse;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.DGP.ServiceBasics.Errors;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

internal sealed class SparkJobManager : ISparkJobManager
{
    private readonly ISparkPoolRepository<SparkPoolModel> sparkPoolRepository;
    private readonly ISynapseSparkExecutor synapseSparkExecutor;
    private readonly IDataEstateHealthRequestLogger logger;

    private readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public SparkJobManager(
        ISparkPoolRepository<SparkPoolModel> sparkPoolRepository,
        ISynapseSparkExecutor synapseSparkExecutor,
        IDataEstateHealthRequestLogger logger)
    {
        this.sparkPoolRepository = sparkPoolRepository;
        this.synapseSparkExecutor = synapseSparkExecutor;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<SparkPoolJobModel> SubmitJob(SparkJobRequest sparkJobRequest, AccountServiceModel accountServiceModel, CancellationToken cancellationToken, ResourceIdentifier existingPoolResourceId = null)
    {
        using (this.logger.LogElapsed("submit job"))
        {
            var sparkPoolId = existingPoolResourceId ?? await this.CreateSparkPoolWithRetry(accountServiceModel, cancellationToken);

            this.logger.LogInformation($"Submitting spark job {sparkJobRequest.Name} to pool={sparkPoolId.Name}");

            try
            {
                var jobId = await this.synapseSparkExecutor.SubmitJob(sparkPoolId.Name, sparkJobRequest, cancellationToken);

                return new SparkPoolJobModel
                {
                    JobId = jobId,
                    PoolResourceId = sparkPoolId
                };
            }
            catch (RequestFailedException ex) when (ex.Status == 404 && existingPoolResourceId != null)
            {
                this.logger.LogWarning($"Failed to submit spark job {sparkJobRequest.Name} to pool={sparkPoolId.Name}. Existing pool not exist. Re-submit the job.", ex);
                return await this.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Failed to submit spark job {sparkJobRequest.Name} to pool={sparkPoolId.Name}.", ex);

                if (existingPoolResourceId == null)
                {
                    // If we created the pool, we should delete it if the job submission fails
                    await this.DeleteSparkPool(sparkPoolId, cancellationToken);
                }
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public async Task CancelJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("cancel job"))
        {
            SparkPoolModel sparkPool = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
            ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(sparkPool);
            this.logger.LogInformation($"Cancelling spark job {batchId} in pool={sparkPoolId.Name}");
            await this.synapseSparkExecutor.CancelJob(sparkPoolId.Name, batchId, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task CancelJob(SparkPoolJobModel jobModel, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("cancel job"))
        {
            ResourceIdentifier sparkPoolId = new ResourceIdentifier(jobModel.PoolResourceId);
            var batchId = int.Parse(jobModel.JobId);
            this.logger.LogInformation($"Cancelling spark job {batchId} in pool={sparkPoolId.Name}");
            await this.synapseSparkExecutor.CancelJob(sparkPoolId.Name, batchId, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken)
    {
        SparkPoolModel sparkPool = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
        ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(sparkPool);
        this.logger.LogInformation($"Get spark job {batchId} in pool={sparkPoolId.Name}");
        SparkBatchJob response = await this.synapseSparkExecutor.GetJob(sparkPoolId.Name, batchId, cancellationToken);
        this.logger.LogInformation($"Retrieved spark job {JsonSerializer.Serialize(response, this.jsonOptions)} in pool={sparkPoolId.Name}");
        this.logger.LogInformation($"spark job status, tenant: {accountServiceModel.TenantId}, status: {response.Result}, livy: {response.LivyInfo}");

        return response;
    }

    /// <inheritdoc/>
    public async Task<SparkBatchJob> GetJob(SparkPoolJobModel jobModel, CancellationToken cancellationToken)
    {
        ResourceIdentifier sparkPoolId = new ResourceIdentifier(jobModel.PoolResourceId);
        var batchId = int.Parse(jobModel.JobId);

        this.logger.LogInformation($"Get spark job {batchId} in pool={sparkPoolId.Name}");
        SparkBatchJob response = await this.synapseSparkExecutor.GetJob(sparkPoolId.Name, batchId, cancellationToken);
        this.logger.LogInformation($"Retrieved spark job {JsonSerializer.Serialize(response, this.jsonOptions)} in pool={sparkPoolId.Name}");
        this.logger.LogInformation($"spark job status, status: {response.Result}, livy: {response.LivyInfo}");

        return response;
    }

    /// <inheritdoc/>
    public async Task<SparkPoolModel> CreateOrUpdateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("Creating or updating spark pool"))
        {
            SparkPoolModel existingModel = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
            SynapseBigDataPoolInfoData synapseSparkPool;
            SparkPoolModel newModel;
            if (existingModel == null)
            {
                string sparkPoolName = await this.GenerateSparkPoolName(OwnerNames.Health, cancellationToken);
                synapseSparkPool = await this.synapseSparkExecutor.CreateOrUpdateSparkPool(sparkPoolName, accountServiceModel, cancellationToken);
                newModel = ToModel(synapseSparkPool, null, accountServiceModel);

                return await this.sparkPoolRepository.Create(newModel, accountServiceModel.Id, cancellationToken);
            }

            ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(existingModel);
            synapseSparkPool = await this.synapseSparkExecutor.CreateOrUpdateSparkPool(sparkPoolId.Name, accountServiceModel, cancellationToken);
            newModel = ToModel(synapseSparkPool, existingModel, accountServiceModel);

            await this.sparkPoolRepository.Update(newModel, accountServiceModel.Id, cancellationToken);

            return existingModel;
        }
    }

    /// <inheritdoc/>
    public async Task<SparkPoolModel> GetSparkPool(Guid accountId, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("Getting spark pool"))
        {
            SparkPoolLocator storageAccountKey = new(accountId.ToString(), OwnerNames.Health);

            return await this.sparkPoolRepository.GetSingle(storageAccountKey, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteSparkPoolRecord(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("Delete spark pool record"))
        {
            try
            {
                SparkPoolLocator storageAccountKey = new(accountServiceModel.Id, OwnerNames.Health);

                var existingModel = await this.sparkPoolRepository.GetSingle(storageAccountKey, cancellationToken);

                if (existingModel == null)
                {
                    return;
                }

                await this.sparkPoolRepository.Delete(storageAccountKey, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Failed to delete spark pool record", ex);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public async Task DeleteSparkPool(string poolResourceId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(poolResourceId))
        {
            this.logger.LogInformation($"Spark pool {poolResourceId} not exist.");
            return;
        }

        ResourceIdentifier sparkPoolId = new(poolResourceId);

        this.logger.LogInformation($"Start to delete spark pool {poolResourceId}.");

        await this.synapseSparkExecutor.DeleteSparkPool(sparkPoolId.Name, cancellationToken);
    }

    private async Task<ResourceIdentifier> CreateSparkPoolWithRetry(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, int maxAttempts = 3)
    {
        using (this.logger.LogElapsed("Creating spark pool with retry"))
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    this.logger.LogInformation($"Creating spark pool attempt {attempt}. Max attempt {maxAttempts}.");
                    return await this.CreateSparkPool(accountServiceModel, cancellationToken);
                }
                catch (Exception ex)
                {
                    this.logger.LogError($"Failed to create spark pool in attempt {attempt}. Max attempt {maxAttempts}.", ex);
                    await Task.Delay(3000, cancellationToken);
                }
            }

            throw new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.Job_UnhandledError.Code,
                $"Failed to create spark pool after {maxAttempts} attempts.").ToException();
        }
    }

    private async Task<ResourceIdentifier> CreateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("Creating spark pool"))
        {
            string sparkPoolName = await this.GenerateSparkPoolName("v2" + OwnerNames.Health, cancellationToken);
            var sparkPool = await this.synapseSparkExecutor.CreateOrUpdateSparkPool(sparkPoolName, accountServiceModel, cancellationToken);

            this.logger.LogInformation($"Created pool id = {sparkPool.Id}");

            return sparkPool.Id;
        }
    }

    /// <summary>
    /// Generates a valid spark pool name that is available.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<string> GenerateSparkPoolName(string prefix, CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed("generate spark pool name"))
        {
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                string sparkPoolName = SparkUtilities.ConstructSparkPoolName(prefix);
                this.logger.LogInformation($"Checking name availability for spark pool {sparkPoolName} attempt {attempt}");
                bool sparkPool = await this.synapseSparkExecutor.SparkPoolExists(sparkPoolName, cancellationToken);
                if (!sparkPool)
                {
                    return sparkPoolName;
                }
            }

            throw new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.Unknown.Code,
                $"The spark pool name is not available.").ToException();
        }
    }

    private static ResourceIdentifier GetSparkPoolResourceId(SparkPoolModel sparkPoolModel)
    {
        return new ResourceIdentifier(sparkPoolModel.Properties.ResourceId);
    }

    /// <summary>
    /// Convert to the spark pool model.
    /// </summary>
    /// <param name="synapseSparkPool"></param>
    /// <param name="existingModel"></param>
    /// <param name="accountServiceModel"></param>
    /// <returns></returns>
    private static SparkPoolModel ToModel(SynapseBigDataPoolInfoData synapseSparkPool, SparkPoolModel existingModel, AccountServiceModel accountServiceModel)
    {
        DateTime now = DateTime.UtcNow;

        return new()
        {
            AccountId = Guid.Parse(accountServiceModel.Id),
            Id = existingModel?.Id ?? Guid.NewGuid(),
            Name = OwnerNames.Health,
            TenantId = Guid.Parse(accountServiceModel.TenantId),
            Properties = new()
            {
                CreatedAt = existingModel?.Properties?.CreatedAt ?? now,
                LastModifiedAt = now,
                Location = synapseSparkPool.Location,
                ResourceId = synapseSparkPool.Id
            },
        };
    }
}
