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
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DimensionModelSparkJobComponent : IDimensionModelSparkJobComponent
{
    private readonly ISparkJobManager sparkJobManager;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;


    public DimensionModelSparkJobComponent(
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
        KeyVaultSecret cosmosDBKey = await this.keyVaultAccessorService.GetSecretAsync("cosmosDBWritekey", cancellationToken);
        KeyVaultSecret cosmosDBEndpoint = await this.keyVaultAccessorService.GetSecretAsync("cosmosDBEndpoint", cancellationToken);
        KeyVaultSecret workSpaceID = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", cancellationToken);

        Models.ProcessingStorageModel processingStorageModel = await this.processingStorageManager.Get(accountServiceModel, cancellationToken);
        string containerName = accountServiceModel.DefaultCatalogId;
        Uri sinkSasUri = await this.GetSinkSasUri(processingStorageModel, containerName, cancellationToken);
        //TODO: Updated main method
        string jarClassName = "com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.main.DimensionalModelMain";        
        SparkJobRequest sparkJobRequest = this.GetSparkJobRequest(sinkSasUri, processingStorageModel.AccountId.ToString(), containerName, sinkSasUri.Host, jobId, jarClassName, accountServiceModel.TenantId, cosmosDBEndpoint.Value, cosmosDBKey.Value, workSpaceID.Value);


        var poolResourceId = string.IsNullOrEmpty(sparkPoolId) ? null : new ResourceIdentifier(sparkPoolId);

        return await this.sparkJobManager.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken, poolResourceId);
    }

    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken) => await this.sparkJobManager.GetJob(accountServiceModel, batchId, cancellationToken);

    public async Task<SparkBatchJob> GetJob(SparkPoolJobModel jobInfo, CancellationToken cancellationToken) => await this.sparkJobManager.GetJob(jobInfo, cancellationToken);

    private async Task<Uri> GetSinkSasUri(Models.ProcessingStorageModel processingStorageModel, string containerName, CancellationToken cancellationToken)
    {
        Models.StorageSasRequest storageSasRequest = new()
        {
            Path = "/",
            Permissions = "rwdlac",
            TimeToLive = TimeSpan.FromHours(1)
        };

        return await this.processingStorageManager.GetProcessingStorageSasUri(processingStorageModel, storageSasRequest, containerName, cancellationToken);
    }

    private SparkJobRequest GetSparkJobRequest(Uri sasUri, string accountId, string containerName, string sinkLocation, string jobId, string jarClassName, string tenantId, string cosmosDBEndpoint = "", string cosmosDBKey = "", string workSpaceID = "")
    {
        return new()
        {
            Configuration = this.GetSinkConfiguration(sasUri, containerName, cosmosDBEndpoint, cosmosDBKey, workSpaceID, tenantId),
            ExecutorCount = 2,
            File = $"abfss://datadomain@{this.serverlessPoolConfiguration.StorageAccount}.dfs.core.windows.net/dataestatehealthanalytics-dimensionalmodel-azure-purview-1.1-jar.jar",
            Name = $"DimensionModelSparkJob-{accountId}",
            ClassName = jarClassName,
            RunManagerArgument = new List<string>()
            {
                $"--AdlsTargetDirectory", $"abfss://{containerName}@{sinkLocation}",
                $"--ReProcessingThresholdInMins", "0",
                $"--AccountId", $"{accountId}",
                $"--JobRunGuid", $"{jobId}"
            },
        };
    }

    private Dictionary<string, string> GetSinkConfiguration(Uri sasUri, string containerName, string cosmosDBEndpoint, string cosmosDBKey, string workSpaceID, string tenantId)
    {
        return new Dictionary<string, string>()
        {
            //{$"fs.azure.account.auth.type.{sasUri.Host}", "SAS"},
            //{$"fs.azure.sas.token.provider.type.{sasUri.Host}", "com.microsoft.azure.synapse.tokenlibrary.ConfBasedSASProvider" },
            //{$"spark.storage.synapse.{containerName}.{sasUri.Host}.sas", sasUri.Query[1..] },
            //{$"fs.azure.sas.fixed.token.{sasUri.Host}.dfs.core.windows.net", sasUri.Query[1..]},
            {$"spark.microsoft.delta.optimizeWrite.enabled" ,"true" },
            {$"spark.serializer","org.apache.spark.serializer.KryoSerializer" },
            {$"spark.jars.packages","com.github.scopt:scopt_2.12:4.0.1" },
            {$"spark.dynamicAllocation.enabled", "true" },
            {$"spark.dynamicAllocation.minExecutors","3" },
            {$"spark.dynamicAllocation.maxExecutors","16" },
            {$"spark.dynamicAllocation.executorIdleTimeout","900s" },
            {$"spark.sql.adaptive.enabled", "true" },
            {$"spark.sql.adaptive.skewJoin.enabled", "true" },
            {$"spark.cosmos.accountEndpoint", $"{cosmosDBEndpoint}" },
            {$"spark.cosmos.database", "dgh-DataEstateHealth" },
            //{$"spark.cosmos.accountKey", cosmosDBKey },
            {$"spark.keyvault.name", this.keyVaultBaseURL},
            {$"spark.analyticalcosmos.keyname", "cosmosDBWritekey"},
            //Don't deploy till log analytics is automated
            {$"spark.loganalytics.workspaceid","logAnalyticsWorkspaceId"},
            {$"spark.loganalytics.workspacekeyname", "logAnalyticsKey" },
            {$"spark.synapse.logAnalytics.enabled", "true" },
            {$"spark.synapse.logAnalytics.workspaceId",workSpaceID },
            {$"spark.synapse.logAnalytics.keyVault.name", this.keyVaultBaseURL},
            {$"spark.synapse.logAnalytics.keyVault.key.secret","logAnalyticsKey" },
            {$"spark.ec.deleteModelFolder","true" },
            {$"spark.purview.tenantId",  $"{tenantId}" }

        };
    }
}
