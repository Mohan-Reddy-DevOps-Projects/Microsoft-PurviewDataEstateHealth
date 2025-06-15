// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark;
using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using global::Azure.ResourceManager.Synapse;
using global::Azure.ResourceManager.Synapse.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Common;
using System.Threading;

internal sealed class SynapseSparkExecutor(
    AzureCredentialFactory azureCredentialFactory,
    IAzureResourceManagerFactory azureResourceManagerFactory,
    IOptions<SynapseSparkConfiguration> synapseAuthConfiguration,
    IAccountExposureControlConfigProvider accountExposureControlConfigProvider,
    IDataEstateHealthRequestLogger logger,
    IDataHealthApiService dataHealthApiService) : ISynapseSparkExecutor
{
    private readonly TokenCredential tokenCredential = azureCredentialFactory.CreateDefaultAzureCredential();
    private readonly IAzureResourceManager azureResourceManager = azureResourceManagerFactory.Create<ProcessingStorageAuthConfiguration>();
    private readonly SynapseSparkConfiguration synapseSparkConfiguration = synapseAuthConfiguration.Value;
    private readonly IDataHealthApiService dataHealthApiService = dataHealthApiService;

    /// <inheritdoc/>
    public async Task<SynapseBigDataPoolInfoData> CreateOrUpdateSparkPool(string sparkPoolName, AccountServiceModel accountServiceModel, CancellationToken cancellationToken, Action<SynapseBigDataPoolInfoData> configAction = null)
    {
        var info = DefaultSparkConfig(this.synapseSparkConfiguration.AzureRegion, configAction);

        this.TweakDefaultSparkConfig(info, accountServiceModel);

        //Commenting this as DQ will not honor the SKU to run Health Controls jobs and so DEH will miss the billing opportunity based of SKU
        //Retaining the code for future if required
        //try
        //{
        //    var SKUConfig = await this.GetDEHSKUConfigByAccount(accountServiceModel.Id.ToString());
        //    switch (SKUConfig.ToLower())
        //    {
        //        case "basic":
        //            info.NodeSize = BigDataPoolNodeSize.Small;
        //            break;
        //        case "standard":
        //            info.NodeSize = BigDataPoolNodeSize.Large;
        //            break;
        //        case "advanced":
        //            info.NodeSize = BigDataPoolNodeSize.XLarge;
        //            break;
        //        default:
        //            info.NodeSize = BigDataPoolNodeSize.Small;
        //            break;
        //    }
        //    logger.LogInformation($"Catalog Spark SKU set to :{info.NodeSize.ToString()} for Account Id: {accountServiceModel.Id}.");
        //}
        //catch (Exception ex)
        //{
        //    logger.LogWarning($"Unable to get Catalog DEH SKU config, setting the default SKU to small : {accountServiceModel.Id}.", ex);
        //}

        return await this.azureResourceManager.CreateOrUpdateSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, info, cancellationToken);
    }

    /// <summary>
    /// Get DEH SKU Configuration
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    private async Task<string> GetDEHSKUConfigByAccount(string accountId)
    {
        //Get DEH SKU Configuration        
        var returnConfig = await this.dataHealthApiService.GetDEHSKUConfig(accountId);
        return returnConfig;
    }


    /// <inheritdoc/>
    public async Task<SynapseBigDataPoolInfoData> GetSparkPool(string sparkPoolName, CancellationToken cancellationToken)
    {
        return await this.azureResourceManager.GetSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<List<SynapseBigDataPoolInfoData>> ListSparkPools(CancellationToken cancellationToken)
    {
        return await this.azureResourceManager.ListSparkPools(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task DeleteSparkPool(string sparkPoolName, CancellationToken cancellationToken)
    {
        await this.azureResourceManager.DeleteSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> SparkPoolExists(string sparkPoolName, CancellationToken cancellationToken)
    {
        return await this.azureResourceManager.SparkPoolExists(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string> SubmitJob(string sparkPoolName, SparkJobRequest sparkJobRequest, CancellationToken cancellationToken)
    {
        SynapseBigDataPoolInfoData sparkPool = await this.azureResourceManager.GetSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, cancellationToken);
        SparkPoolConfigs.Options.TryGetValue(sparkPool.NodeSize.ToString(), out SparkPoolConfigProperties poolConfig);
        SparkBatchJobOptions request = CreateSparkJobRequestOptions(sparkJobRequest, poolConfig);
        SparkBatchClient client = this.GetSparkBatchClient(sparkPoolName);
        SparkBatchOperation createOperation = await client.StartCreateSparkBatchJobAsync(request, cancellationToken: cancellationToken);

        return createOperation.Id;
    }

    public async Task CancelJob(string sparkPoolName, int batchId, CancellationToken cancellationToken)
    {
        SparkBatchClient client = this.GetSparkBatchClient(sparkPoolName);
        await client.CancelSparkBatchJobAsync(batchId, cancellationToken: cancellationToken);
    }

    public async Task<SparkBatchJob> GetJob(string sparkPoolName, int batchId, CancellationToken cancellationToken)
    {
        SparkBatchClient client = this.GetSparkBatchClient(sparkPoolName);
        return (await client.GetSparkBatchJobAsync(batchId, true, cancellationToken)).Value;
    }

    public async Task<List<SparkBatchJob>> ListJobs(string sparkPoolName, CancellationToken cancellationToken)
    {
        SparkBatchClient client = this.GetSparkBatchClient(sparkPoolName);
        var resp = await client.GetSparkBatchJobsAsync(cancellationToken: cancellationToken);
        return resp.Value.Sessions?.ToList();
    }

    private SparkBatchClient GetSparkBatchClient(string sparkPoolName)
    {
        string endpoint = $"https://{this.synapseSparkConfiguration.Workspace}.dev.azuresynapse.net";
        return new(new Uri(endpoint), sparkPoolName, this.tokenCredential);
    }

    private static SparkBatchJobOptions CreateSparkJobRequestOptions(SparkJobRequest sparkJobRequest, SparkPoolConfigProperties poolConfig)
    {
        SparkBatchJobOptions request = new(sparkJobRequest.Name, sparkJobRequest.File)
        {
            ClassName = string.IsNullOrEmpty(sparkJobRequest.ClassName) ? "sample" : sparkJobRequest.ClassName,
            DriverMemory = poolConfig.DriverMemorySize,
            DriverCores = poolConfig.DriverCores,
            ExecutorMemory = poolConfig.ExecutorMemorySize,
            ExecutorCores = poolConfig.ExecutorCores,
            ExecutorCount = sparkJobRequest.ExecutorCount,
        };


        foreach (string argv in sparkJobRequest.RunManagerArgument)
        {
            request.Arguments.Add(argv);
        }

        foreach (KeyValuePair<string, string> item in sparkJobRequest.Configuration)
        {
            request.Configuration.Add(item.Key, item.Value);
        }

        return request;
    }

    private static SynapseBigDataPoolInfoData DefaultSparkConfig(string location, Action<SynapseBigDataPoolInfoData> configAction = null)
    {
        var info = new SynapseBigDataPoolInfoData(new AzureLocation(location))
        {
            AutoScale = new BigDataPoolAutoScaleProperties()
            {
                MinNodeCount = 3,
                IsEnabled = true,
                MaxNodeCount = 50,
            },
            AutoPause = new BigDataPoolAutoPauseProperties()
            {
                DelayInMinutes = 15,
                IsEnabled = true,
            },
            IsAutotuneEnabled = false,
            IsSessionLevelPackagesEnabled = true,
            DynamicExecutorAllocation = new SynapseDynamicExecutorAllocation()
            {
                IsEnabled = true,
                MinExecutors = 1,
                MaxExecutors = 4,
            },
            SparkEventsFolder = "/events",
            NodeCount = 4,
            SparkVersion = "3.4",
            DefaultSparkLogFolder = "/logs",
            NodeSize = BigDataPoolNodeSize.Medium,
            NodeSizeFamily = BigDataPoolNodeSizeFamily.MemoryOptimized
        };

        configAction?.Invoke(info);

        return info;
    }

    private void TweakDefaultSparkConfig(SynapseBigDataPoolInfoData info, AccountServiceModel accountServiceModel)
    {
        var methodName = nameof(TweakDefaultSparkConfig);
        var tenantId = accountServiceModel.TenantId;
        logger.LogInformation($"[{methodName}] TenantID: {tenantId}");

        // above hard code config can be overridden by the following account exposure control config

        var configs = accountExposureControlConfigProvider.GetDGSparkJobConfig();
        var config = configs.TryGetValue(tenantId, out var value) ? value : null;

        if (config == null)
        {
            logger.LogInformation($@"[{methodName}] EC dictionary ""DGSparkJobConfig"" has no value, skip tweaking.");
            return;
        }

        if (config.MaxCapacityUnits.HasValue)
        {
            logger.LogInformation($@"[{methodName}] MaxCapacityUnits sets to {config.MaxCapacityUnits.Value}");
            info.AutoScale.MaxNodeCount = config.MaxCapacityUnits.Value;
        }
        else
        {
            logger.LogInformation($@"[{methodName}] No MaxCapacityUnits");
        }

        if (config.NodeSize != null)
        {
            logger.LogInformation($@"[{methodName}] NodeSize sets to {config.NodeSize}");
            // string will be implicitly converted to BigDataPoolNodeSize
            info.NodeSize = config.NodeSize;
        }
        else
        {
            logger.LogInformation($@"[{methodName}] No NodeSize");
        }
    }
}
