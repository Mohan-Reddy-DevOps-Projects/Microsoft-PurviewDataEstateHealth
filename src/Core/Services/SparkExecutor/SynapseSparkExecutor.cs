// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Text;
using System.Text.Json;
using System.Threading;
using global::Azure.Analytics.Synapse.Spark;
using global::Azure.Analytics.Synapse.Spark.Models;
using global::Azure.Core;
using global::Azure.ResourceManager.Synapse;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;
using Microsoft.Extensions.Options;

internal sealed class SynapseSparkExecutor : ISynapseSparkExecutor
{
    private readonly TokenCredential tokenCredential;
    private readonly IAzureResourceManager azureResourceManager;
    private readonly SynapseSparkConfiguration synapseSparkConfiguration;

    public SynapseSparkExecutor(
        ISparkPoolRepository<SparkPoolModel> sparkPoolRepository,
        AzureCredentialFactory azureCredentialFactory,
        IAzureResourceManagerFactory azureResourceManagerFactory,
        IOptions<SynapseSparkConfiguration> synapseAuthConfiguration)
    {
        this.azureResourceManager = azureResourceManagerFactory.Create<ProcessingStorageAuthConfiguration>();
        this.synapseSparkConfiguration = synapseAuthConfiguration.Value;
        this.tokenCredential = azureCredentialFactory.CreateDefaultAzureCredential();
    }

    /// <inheritdoc/>
    public async Task<SynapseBigDataPoolInfoData> CreateOrUpdateSparkPool(string sparkPoolName, CancellationToken cancellationToken)
    {
        return await this.azureResourceManager.CreateOrUpdateSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, this.synapseSparkConfiguration.AzureRegion, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<SynapseBigDataPoolInfoData> GetSparkPool(string sparkPoolName, CancellationToken cancellationToken)
    {
        return await this.azureResourceManager.GetSparkPool(this.synapseSparkConfiguration.SubscriptionId, this.synapseSparkConfiguration.ResourceGroup, this.synapseSparkConfiguration.Workspace, sparkPoolName, cancellationToken);
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

    private SparkBatchClient GetSparkBatchClient(string sparkPoolName)
    {
        string endpoint = $"https://{this.synapseSparkConfiguration.Workspace}.dev.azuresynapse.net";
        return new(new Uri(endpoint), sparkPoolName, this.tokenCredential);
    }

    private static SparkBatchJobOptions CreateSparkJobRequestOptions(SparkJobRequest sparkJobRequest, SparkPoolConfigProperties poolConfig)
    {
        SparkBatchJobOptions request = new(sparkJobRequest.Name, sparkJobRequest.File)
        {
            ClassName = "sample",
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
}
