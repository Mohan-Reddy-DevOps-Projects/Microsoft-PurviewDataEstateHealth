// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ComputeGovernedAssetsSparkJobComponent(
    ISparkJobManager sparkJobManager,
    IKeyVaultAccessorService keyVaultAccessorService,
    IOptions<KeyVaultConfiguration> keyVaultConfig,
    IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration,
    IMetadataAccessorService metadataAccessorService,
    IProcessingStorageManager processingStorageManager) : IComputeGovernedAssetsSparkJobComponent
{

    /// <inheritdoc/>
    public async Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, string jobId, string sparkPoolId)
    {
        KeyVaultSecret workSpaceID = await keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", cancellationToken);
        var keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();

        var containerName = accountServiceModel.DefaultCatalogId;
        var rddHost = $"{accountServiceModel.ProcessingStorageModel?.Name}.{accountServiceModel.ProcessingStorageModel?.DnsZone}.blob.storage.azure.net";
        var rddAssetsFormat = "delta";
        var rddAssetsFolderPath = "AtlasRdd/AtlasDeltaDataset";
        var rddAssetsPath = $"wasbs://{containerName}@{rddHost}/{rddAssetsFolderPath}";

        var storageTokenKey = await metadataAccessorService.GetProcessingStorageDelegationSasToken(Guid.Parse(accountServiceModel.Id), accountServiceModel.DefaultCatalogId, "racwdli", cancellationToken);

        var dgAccountStorageModel = await processingStorageManager.Get(new Guid(accountServiceModel.Id), CancellationToken.None).ConfigureAwait(false);
        var domainModelAssetsFormat = "delta";
        var domainModelAssetsPath = $"abfss://{containerName}@{dgAccountStorageModel.GetStorageAccountName()}.{dgAccountStorageModel.Properties.DnsZone}.dfs.{dgAccountStorageModel.Properties.EndpointSuffix}/DomainModel/DataAsset";

        SparkJobRequest sparkJobRequest = new()
        {
            ExecutorCount = 2,
            File = $"abfss://datadomain@{serverlessPoolConfiguration.Value.StorageAccount}.dfs.core.windows.net/dataestatehealthanalytics-computegovernedassets-azure-purview-1.0-jar.jar",
            ClassName = "com.microsoft.azurepurview.dataestatehealth.computegovernedassets.main.ComputeGovernedAssetsMain",
            Name = $"ComputeGovernedAssetsSparkJob-{accountServiceModel.Id}",
            RunManagerArgument =
            [
                $"--AccountId", $"{accountServiceModel.Id}",
                $"--JobRunGuid", jobId
            ],
            Configuration = new Dictionary<string, string>()
            {
                // RDD connection configs
                {$"spark.hadoop.fs.azure.account.auth.type.{rddHost}", "SAS" },
                {$"spark.hadoop.fs.azure.sas.token.provider.type.{rddHost}", "com.microsoft.azure.synapse.tokenlibrary.ConfBasedSASProvider" },
                {$"spark.hadoop.fs.azure.sas.{containerName}.{rddHost}", storageTokenKey.Key },
                {"spark.rdd.assetsFormat", rddAssetsFormat },
                {"spark.rdd.assetsPath", rddAssetsPath },
                // RDD connection configs end
                // Domain model configs
                {"spark.domainModel.assetsFormat", domainModelAssetsFormat },
                {"spark.domainModel.assetsPath", domainModelAssetsPath },
                // Domain model configs end
                {"spark.microsoft.delta.optimizeWrite.enabled" ,"true" },
                {"spark.serializer","org.apache.spark.serializer.KryoSerializer" },
                {"spark.jars.packages","com.github.scopt:scopt_2.12:4.0.1" },
                {"spark.dynamicAllocation.enabled", "true" },
                {"spark.dynamicAllocation.minExecutors","1" },
                {"spark.dynamicAllocation.maxExecutors","2" },
                {"spark.dynamicAllocation.executorIdleTimeout","900s" },
                {"spark.sql.adaptive.enabled", "true" },
                {"spark.sql.adaptive.skewJoin.enabled", "true" },
                //{$"spark.cosmos.accountEndpoint", $"{cosmosDBEndpoint}" },
                {"spark.cosmos.database", "dgh-DataEstateHealth" },
                //{$"spark.cosmos.accountKey", cosmosDBKey },
                {"spark.keyvault.name", keyVaultBaseURL},
                {"spark.analyticalcosmos.keyname", "cosmosDBWritekey"},
                //Don't deploy till log analytics is automated
                {"spark.loganalytics.workspaceid","logAnalyticsWorkspaceId"},
                {"spark.loganalytics.workspacekeyname", "logAnalyticsKey" },
                {"spark.synapse.logAnalytics.enabled", "true" },
                {"spark.synapse.logAnalytics.workspaceId",workSpaceID.Value },
                {"spark.synapse.logAnalytics.keyVault.name", keyVaultBaseURL},
                {"spark.synapse.logAnalytics.keyVault.key.secret","logAnalyticsKey" },
            }
        };

        var poolResourceId = string.IsNullOrEmpty(sparkPoolId) ? null : new ResourceIdentifier(sparkPoolId);

        return await sparkJobManager.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken, poolResourceId);
    }

    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken) => await sparkJobManager.GetJob(accountServiceModel, batchId, cancellationToken);

    public async Task<SparkBatchJob> GetJob(SparkPoolJobModel jobInfo, CancellationToken cancellationToken) => await sparkJobManager.GetJob(jobInfo, cancellationToken);
}
