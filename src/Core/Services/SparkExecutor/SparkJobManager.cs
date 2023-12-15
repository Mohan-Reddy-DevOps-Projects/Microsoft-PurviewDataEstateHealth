// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using global::Azure.ResourceManager.Synapse;
using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Errors;
using System.Text.Json;
using System.Text.Json.Serialization;

internal sealed class SparkJobManager : ISparkJobManager
{
    private const string sparkPoolPrefix = "health";
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
    public async Task<string> SubmitJob(AccountServiceModel accountServiceModel, SparkJobRequest sparkJobRequest, CancellationToken cancellationToken)
    {
        SparkPoolModel sparkPool = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
        ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(sparkPool);

        this.logger.LogInformation($"Submitting spark job {sparkJobRequest.Name} to pool={sparkPoolId.Name}");
        return await this.synapseSparkExecutor.SubmitJob(sparkPoolId.Name, sparkJobRequest, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CancelJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken)
    {
        SparkPoolModel sparkPool = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
        ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(sparkPool);
        this.logger.LogInformation($"Cancelling spark job {batchId} in pool={sparkPoolId.Name}");
        await this.synapseSparkExecutor.CancelJob(sparkPoolId.Name, batchId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken)
    {
        SparkPoolModel sparkPool = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
        ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(sparkPool);
        this.logger.LogInformation($"Get spark job {batchId} in pool={sparkPoolId.Name}");
        SparkBatchJob response = await this.synapseSparkExecutor.GetJob(sparkPoolId.Name, batchId, cancellationToken);
        this.logger.LogInformation($"Retrieved spark job {JsonSerializer.Serialize(response, jsonOptions)} in pool={sparkPoolId.Name}");

        return response;
    }

    /// <inheritdoc/>
    public async Task<SparkPoolModel> CreateOrUpdateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Creating or updating spark pool");

        SparkPoolModel existingModel = await this.GetSparkPool(Guid.Parse(accountServiceModel.Id), cancellationToken);
        SynapseBigDataPoolInfoData synapseSparkPool;
        SparkPoolModel newModel;
        if (existingModel == null)
        {
            string sparkPoolName = await this.GenerateSparkPoolName(sparkPoolPrefix, cancellationToken);
            synapseSparkPool = await this.synapseSparkExecutor.CreateOrUpdateSparkPool(sparkPoolName, cancellationToken);
            newModel = ToModel(synapseSparkPool, null, accountServiceModel);

            return await this.sparkPoolRepository.Create(newModel, accountServiceModel.Id, cancellationToken);
        }

        ResourceIdentifier sparkPoolId = GetSparkPoolResourceId(existingModel);
        synapseSparkPool = await this.synapseSparkExecutor.CreateOrUpdateSparkPool(sparkPoolId.Name, cancellationToken);
        newModel = ToModel(synapseSparkPool, existingModel, accountServiceModel);

        await this.sparkPoolRepository.Update(newModel, accountServiceModel.Id, cancellationToken);

        return existingModel;
    }

    /// <inheritdoc/>
    public async Task<SparkPoolModel> GetSparkPool(Guid accountId, CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Getting spark pool");
        SparkPoolLocator storageAccountKey = new(accountId.ToString(), sparkPoolPrefix);

        return await this.sparkPoolRepository.GetSingle(storageAccountKey, cancellationToken);
    }


    /// <summary>
    /// Generates a valid spark pool name that is available.
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<string> GenerateSparkPoolName(string prefix, CancellationToken cancellationToken)
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
            Name = sparkPoolPrefix,
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
