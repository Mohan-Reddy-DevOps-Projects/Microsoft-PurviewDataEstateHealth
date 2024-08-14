// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ComputeGovernedAssetsSparkJobComponent : IComputeGovernedAssetsSparkJobComponent
{
    private readonly ISparkJobManager sparkJobManager;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;


    public ComputeGovernedAssetsSparkJobComponent(
        ISparkJobManager sparkJobManager,
        IProcessingStorageManager processingStorageManager,
        IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration,
        IKeyVaultAccessorService keyVaultAccessorService,
        IOptions<KeyVaultConfiguration> keyVaultConfig)
    {
        this.sparkJobManager = sparkJobManager;
        this.processingStorageManager = processingStorageManager;
        this.serverlessPoolConfiguration = serverlessPoolConfiguration.Value;
        this.keyVaultAccessorService = keyVaultAccessorService;
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();
    }

    /// <inheritdoc/>
    public async Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, string jobId, string sparkPoolId)
    {
        SparkJobRequest sparkJobRequest = new()
        {
            ExecutorCount = 2,
            File = $"abfss://jartest@dghdogfoodsynapse.dfs.core.windows.net/test.py",
            Name = "ComputingAssetSparkJob",
            RunManagerArgument = new List<string>()
            {
                $"--AccountId", $"{accountServiceModel.Id}",
                $"--JobRunGuid", jobId
            },
            Configuration = new Dictionary<string, string>()
            {
                { "rdd.directory", $"abfss://{accountServiceModel.DefaultCatalogId}@{accountServiceModel.ProcessingStorageModel?.Name}.{accountServiceModel.ProcessingStorageModel?.DnsZone}.dfs.storage.azure.net/AtlasRdd/AtlasDeltaDataset" },
                // TODO
                { "rdd.sasToken", string.Empty },
            }
        };

        var poolResourceId = string.IsNullOrEmpty(sparkPoolId) ? null : new ResourceIdentifier(sparkPoolId);

        return await this.sparkJobManager.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken, poolResourceId);
    }

    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken) => await this.sparkJobManager.GetJob(accountServiceModel, batchId, cancellationToken);

    public async Task<SparkBatchJob> GetJob(SparkPoolJobModel jobInfo, CancellationToken cancellationToken) => await this.sparkJobManager.GetJob(jobInfo, cancellationToken);
}
