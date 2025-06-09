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
using Microsoft.Purview.DataGovernance.SynapseSqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ControlsWorkflowSparkJobComponent(
    ISparkJobManager sparkJobManager,
    IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration,
    IKeyVaultAccessorService keyVaultAccessorService,
    IOptions<KeyVaultConfiguration> keyVaultConfig,
    IProcessingStorageManager processingStorageManager) : IControlsWorkflowSparkJobComponent
{
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration = serverlessPoolConfiguration.Value;
    private readonly string keyVaultBaseUrl = keyVaultConfig.Value.BaseUrl;

    /// <inheritdoc/>
    public async Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, string jobId, string sparkPoolId, bool isDEHDataCleanup)
    {
        var cosmosDbKey = await keyVaultAccessorService.GetSecretAsync("cosmosDBWritekey", cancellationToken);
        var cosmosDbEndpoint = await keyVaultAccessorService.GetSecretAsync("cosmosDBEndpoint", cancellationToken);
        var workSpaceId = await keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", cancellationToken);

        // Get processing storage model and SAS URI for storage authentication
        var processingStorageModel = await processingStorageManager.Get(accountServiceModel, cancellationToken);
        const string containerName = "deh"; // Controls workflow uses "deh" container
        var sinkSasUri = await this.GetSinkSasUri(processingStorageModel, containerName, cancellationToken);

        var sparkJobRequest = this.GetSparkJobRequest(accountServiceModel.Id, jobId, accountServiceModel.TenantId, isDEHDataCleanup, cosmosDbEndpoint.Value, cosmosDbKey.Value, workSpaceId.Value, sinkSasUri, containerName);
        
        var poolResourceId = String.IsNullOrEmpty(sparkPoolId) ? null : new ResourceIdentifier(sparkPoolId);
        
        return await sparkJobManager.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken, poolResourceId);
    }

    /// <inheritdoc/>
    public async Task<SparkBatchJob> GetJob(AccountServiceModel accountServiceModel, int batchId, CancellationToken cancellationToken)
    {
        return await sparkJobManager.GetJob(accountServiceModel, batchId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SparkBatchJob> GetJob(SparkPoolJobModel jobInfo, CancellationToken cancellationToken)
    {
        return await sparkJobManager.GetJob(jobInfo, cancellationToken);
    }

    private async Task<Uri> GetSinkSasUri(Models.ProcessingStorageModel processingStorageModel, string containerName, CancellationToken cancellationToken)
    {
        Models.StorageSasRequest storageSasRequest = new()
        {
            Path = "/",
            Permissions = "rwdlac",
            TimeToLive = TimeSpan.FromHours(1)
        };

        return await processingStorageManager.GetProcessingStorageSasUri(processingStorageModel, storageSasRequest, containerName, cancellationToken);
    }

    private SparkJobRequest GetSparkJobRequest(string accountId,
        string jobId,
        string tenantId,
        bool isDEHDataCleanup,
        string cosmosDBEndpoint,
        string cosmosDBKey,
        string workSpaceID,
        Uri sasUri,
        string containerName)
    {
        return new SparkJobRequest
        {
            // Use the specified JAR file
            File = $"abfss://datadomain@{this.serverlessPoolConfiguration.StorageAccount}.dfs.core.windows.net/dataestatehealthanalytics-azure-purview-controls-1.1.jar",
            
            // Use the specified main class
            ClassName = "com.microsoft.azurepurview.dataestatehealth.controls.main.ControlsMain",
            
            // Set job name with account ID for easier tracking
            Name = $"ControlsWorkflowJob-{accountId}",
            
            // Set executor count
            ExecutorCount = 3,
            
            // Configure command line arguments
            RunManagerArgument = new List<string>
            {
                "--AdlsTargetDirectory", $"abfss://{containerName}@{sasUri.Host}/DomainModel",
                "--AccountId", accountId,
                "--RefreshType", "Incremental",
                "--ReProcessingThresholdInMins", "0",
                "--JobRunGuid", jobId
            },
            
            // Configure Spark settings
            Configuration = this.GetStorageConfiguration(sasUri, containerName, cosmosDBEndpoint, cosmosDBKey, workSpaceID, tenantId, isDEHDataCleanup)
        };
    }

    private Dictionary<string, string> GetStorageConfiguration(Uri sasUri, string containerName, string cosmosDBEndpoint, string cosmosDBKey, string workSpaceID, string tenantId, bool isDEHDataCleanup)
    {
        return new Dictionary<string, string>
        {
            // Azure Storage authentication configuration
            // { $"fs.azure.account.auth.type.{sasUri.Host}", "SAS" },
            // { $"fs.azure.sas.token.provider.type.{sasUri.Host}", "com.microsoft.azure.synapse.tokenlibrary.ConfBasedSASProvider" },
            { $"spark.storage.synapse.{containerName}.{sasUri.Host}.sas", sasUri.Query[1..] },
            // { $"fs.azure.sas.fixed.token.{sasUri.Host}", sasUri.Query[1..] },
            
            // Delta and serialization settings
            { "spark.microsoft.delta.optimizeWrite.enabled", "true" },
            { "spark.serializer", "org.apache.spark.serializer.KryoSerializer" },
            { "spark.jars.packages", "com.github.scopt:scopt_2.12:4.0.1" },
            
            // Dynamic allocation settings
            { "spark.dynamicAllocation.enabled", "true" },
            { "spark.dynamicAllocation.minExecutors", "3" },
            { "spark.dynamicAllocation.maxExecutors", "16" },
            { "spark.dynamicAllocation.executorIdleTimeout", "900s" },
            
            // SQL adaptive settings
            { "spark.sql.adaptive.enabled", "true" },
            { "spark.sql.adaptive.skewJoin.enabled", "true" },
            
            // CosmosDB settings
            { "spark.cosmos.accountEndpoint", cosmosDBEndpoint },
            { "spark.cosmos.database", "dgh-DataEstateHealth" },
            
            // KeyVault settings
            { "spark.keyvault.name", this.keyVaultBaseUrl },
            { "spark.analyticalcosmos.keyname", "cosmosDBWritekey" },
            
            // Log Analytics settings
            { "spark.loganalytics.workspaceid", "logAnalyticsWorkspaceId" },
            { "spark.loganalytics.workspacekeyname", "logAnalyticsKey" },
            { "spark.synapse.logAnalytics.enabled", "true" },
            { "spark.synapse.logAnalytics.workspaceId", workSpaceID },
            { "spark.synapse.logAnalytics.keyVault.name", this.keyVaultBaseUrl },
            { "spark.synapse.logAnalytics.keyVault.key.secret", "logAnalyticsKey" },
            
            // Application-specific settings
            { "spark.ec.deleteModelFolder", isDEHDataCleanup.ToString() },
            { "spark.purview.tenantId", tenantId }
        };
    }
} 