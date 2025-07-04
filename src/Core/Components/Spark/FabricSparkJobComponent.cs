﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.JobManagerModels;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.SynapseSqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class FabricSparkJobComponent : IFabricSparkJobComponent
{
    private readonly ISparkJobManager sparkJobManager;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly ServerlessPoolConfiguration serverlessPoolConfiguration;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;
    private readonly IDataHealthApiService dataHealthApiService;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IAccountExposureControlConfigProvider exposureControlConfigProvider;

    public FabricSparkJobComponent(
        ISparkJobManager sparkJobManager,
        IProcessingStorageManager processingStorageManager,
        IOptions<ServerlessPoolConfiguration> serverlessPoolConfiguration,
        IKeyVaultAccessorService keyVaultAccessorService,
        IOptions<KeyVaultConfiguration> keyVaultConfig,
        IDataHealthApiService dataHealthApiService,
        IDataEstateHealthRequestLogger logger,
        IAccountExposureControlConfigProvider exposureControlConfigProvider)
    {
        this.sparkJobManager = sparkJobManager;
        this.processingStorageManager = processingStorageManager;
        this.serverlessPoolConfiguration = serverlessPoolConfiguration.Value;
        this.keyVaultAccessorService = keyVaultAccessorService;
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();
        this.dataHealthApiService = dataHealthApiService;
        this.logger = logger;
        this.exposureControlConfigProvider = exposureControlConfigProvider;
    }

    /// <inheritdoc/>
    //public async Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    public async Task<SparkPoolJobModel> SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken, string jobId, string sparkPoolId, string traceId = null)
    {
        KeyVaultSecret cosmosDBKey = await this.keyVaultAccessorService.GetSecretAsync("cosmosDBWritekey", cancellationToken);
        KeyVaultSecret cosmosDBEndpoint = await this.keyVaultAccessorService.GetSecretAsync("cosmosDBEndpoint", cancellationToken);
        KeyVaultSecret workSpaceID = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", cancellationToken);

        using (this.logger.LogElapsed($"{this.GetType().Name}: Submit Fabric Spark job {accountServiceModel.Id}"))
        {
            Models.ProcessingStorageModel processingStorageModel = await this.processingStorageManager.Get(accountServiceModel, cancellationToken);
            string containerName = accountServiceModel.DefaultCatalogId;
            Uri sinkSasUri = await this.GetSinkSasUri(processingStorageModel, containerName, cancellationToken);
            string jarClassName = "com.microsoft.azurepurview.dataestatehealth.storagesync.main.StorageSyncMain";
            var miToken = "";
            miToken = await this.GetMIToken(accountServiceModel.Id.ToString());
            StorageConfiguration storageConfig = new StorageConfiguration();
            storageConfig = await this.GetStorageConfigSettings(accountServiceModel.Id.ToString(), accountServiceModel.TenantId);
            if (storageConfig != null && string.IsNullOrEmpty(storageConfig.TypeProperties.LocationURL))
            {
                storageConfig.TypeProperties.LocationURL = storageConfig.TypeProperties.Endpoint;
            }
            if (storageConfig == null || string.IsNullOrEmpty(storageConfig?.TypeProperties.LocationURL) || string.IsNullOrEmpty(miToken))
            {
                this.logger.LogInformation($"SubmitFabricJob |BYOC is not configured: accountID:  {processingStorageModel.AccountId.ToString()}");
                return null;
            }
            this.logger.LogInformation($"SubmitJob|StorageConfig: {storageConfig}, accountid: {accountServiceModel.Id}");
            this.logger.LogInformation($"SubmitJob|MiToken: {string.IsNullOrEmpty(miToken)}, accountid: {accountServiceModel.Id}");

            SparkJobRequestModel sparkJobRequestModel = new SparkJobRequestModel
            {
                sasUri = sinkSasUri,
                accountId = processingStorageModel.AccountId.ToString(),
                containerName = containerName,
                sinkLocation = sinkSasUri.Host,
                jobId = jobId,
                jarClassName = jarClassName,
                miToken = miToken,
                storageUrl = storageConfig.TypeProperties.LocationURL,
                storageType = storageConfig.Type,
                cosmosDBEndpoint = cosmosDBEndpoint.Value,
                cosmosDBKey = cosmosDBKey.Value,
                workSpaceID = workSpaceID.Value,
                tenantId = accountServiceModel.TenantId,
                traceId = traceId
            };

            SparkJobRequest sparkJobRequest = this.GetSparkJobRequest(sparkJobRequestModel);
            //sinkSasUri, processingStorageModel.AccountId.ToString(), containerName, sinkSasUri.Host, jobId, jarClassName, miToken, fabricConfig, cosmosDBEndpoint.Value, cosmosDBKey.Value, workSpaceID.Value);
            var poolResourceId = string.IsNullOrEmpty(sparkPoolId) ? null : new ResourceIdentifier(sparkPoolId);
            return await this.sparkJobManager.SubmitJob(sparkJobRequest, accountServiceModel, cancellationToken, poolResourceId);
        }
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

    //Uri sasUri, string accountId, string containerName, string sinkLocation, string jobId, string jarClassName, string miToken, string fabricConfig, string cosmosDBEndpoint = "", string cosmosDBKey = "", string workSpaceID = "")
    private SparkJobRequest GetSparkJobRequest(SparkJobRequestModel sparkJobRequestModel)

    {
        return new()
        {
            //Configuration = this.GetSinkConfiguration(sasUri, containerName),
            Configuration = this.GetSinkConfiguration(sparkJobRequestModel), //accountId, sasUri, containerName, cosmosDBEndpoint, cosmosDBKey, workSpaceID, miToken),
            ExecutorCount = 2,
            File = $"abfss://datadomain@{this.serverlessPoolConfiguration.StorageAccount}.dfs.core.windows.net/dataestatehealthanalytics-azure-purview-storagesync-1.0.jar",
            Name = $"FabricSparkJob-{sparkJobRequestModel.accountId}",
            ClassName = sparkJobRequestModel.jarClassName,
            RunManagerArgument = new List<string>()
            {
                $"--DEHStorageAccount", $"abfss://{sparkJobRequestModel.containerName}@{sparkJobRequestModel.sinkLocation}",
                $"--SyncRootPath", $"{sparkJobRequestModel.storageUrl}",
                $"--SyncType", $"{sparkJobRequestModel.storageType}",
                $"--AccountId", $"{sparkJobRequestModel.accountId}",
                $"--JobRunGuid", $"{sparkJobRequestModel.jobId}",
            },
        };
    }


    private async Task<string> GetMIToken(string accountId)
    {
        //Get MI token
        var returnToken = await this.dataHealthApiService.GetMIToken(accountId);
        return returnToken;
    }

    private async Task<StorageConfiguration> GetStorageConfigSettings(string accountId, string tenantId)
    {
        //Get Fabric Configuration
        StorageConfiguration returnConfig = new StorageConfiguration();
        returnConfig = await this.dataHealthApiService.GetStorageConfigSettings(accountId, tenantId);
        return returnConfig;
    }

    //private Dictionary<string, string> GetSinkConfiguration(Uri sasUri, string containerName)
    //string accountId, Uri sasUri, string containerName, string cosmosDBEndpoint, string cosmosDBKey, string workSpaceID, string miToken)
    private Dictionary<string, string> GetSinkConfiguration(SparkJobRequestModel sparkJobRequestModel)
    {
        var configuration = new Dictionary<string, string>()
        {
            {$"spark.microsoft.delta.optimizeWrite.enabled" ,"true" },
            {$"spark.serializer","org.apache.spark.serializer.KryoSerializer" },
            {$"spark.jars.packages","com.github.scopt:scopt_2.12:4.0.1" },
            {$"spark.dynamicAllocation.enabled", "true" },
            {$"spark.dynamicAllocation.minExecutors","3" },
            {$"spark.dynamicAllocation.maxExecutors","16" },
            {$"spark.dynamicAllocation.executorIdleTimeout","900s" },
            {$"spark.sql.adaptive.enabled", "true" },
            {$"spark.sql.adaptive.skewJoin.enabled", "true" },
            {$"spark.cosmos.accountEndpoint", $"{sparkJobRequestModel.cosmosDBEndpoint}" },
            {$"spark.cosmos.database", "dgh-DataEstateHealth" },
            {$"spark.keyvault.name", this.keyVaultBaseURL},
            {$"spark.analyticalcosmos.keyname", "cosmosDBWritekey"},
            //Don't deploy till log analytics is automated
            {$"spark.loganalytics.workspaceid","logAnalyticsWorkspaceId"},
            {$"spark.loganalytics.workspacekeyname", "logAnalyticsKey" },
            {$"spark.synapse.logAnalytics.enabled", "true" },
            {$"spark.synapse.logAnalytics.workspaceId",sparkJobRequestModel.workSpaceID },
            {$"spark.synapse.logAnalytics.keyVault.name", this.keyVaultBaseURL},
            {$"spark.synapse.logAnalytics.keyVault.key.secret","logAnalyticsKey" },
            {$"spark.mitoken.value",$"{sparkJobRequestModel.miToken}" },
            {$"spark.purview.tenantId",  $"{sparkJobRequestModel.tenantId}" }
        };

        // Add correlation ID - generate one if not provided
        var correlationId = !string.IsNullOrEmpty(sparkJobRequestModel.traceId) ? sparkJobRequestModel.traceId : Guid.NewGuid().ToString();
        configuration.Add("spark.correlationId", correlationId);

        // Add switchToNewControlsFlow feature flag - defaults to false if feature flag check fails
        bool switchToNewControlsFlow = false;
        try
        {
            if (!string.IsNullOrEmpty(sparkJobRequestModel.accountId))
            {
                switchToNewControlsFlow = this.exposureControlConfigProvider.IsDehEnableNewControlsFlowEnabled(sparkJobRequestModel.accountId, string.Empty, sparkJobRequestModel.tenantId);
            }
        }
        catch
        {
            // Default to false if feature flag check fails
            switchToNewControlsFlow = false;
        }
        configuration.Add("spark.ec.switchToNewControlsFlow", switchToNewControlsFlow.ToString().ToLower());

        return configuration;
    }
}
