// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using global::Azure.Core;
using global::Azure.ResourceManager.Synapse;
using global::Azure.ResourceManager;
using global::Azure;
using global::Azure.ResourceManager.Synapse.Models;

internal sealed partial class AzureResourceManager<TAuthConfig>
{
    public async Task<SynapseBigDataPoolInfoData> CreateSparkPool(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, string location, CancellationToken cancellationToken)
    {
        ResourceIdentifier synapseWorkspaceResourceId = SynapseWorkspaceResource.CreateResourceIdentifier(subscriptionId.ToString(), resourceGroupName, workspaceName);
        SynapseWorkspaceResource synapseWorkspace = this.armClient.GetSynapseWorkspaceResource(synapseWorkspaceResourceId);
        SynapseBigDataPoolInfoCollection collection = synapseWorkspace.GetSynapseBigDataPoolInfos();
        SynapseBigDataPoolInfoData info = DefaultSparkConfig(location);
        ArmOperation<SynapseBigDataPoolInfoResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, bigDataPoolName, info, cancellationToken: cancellationToken);

        return lro.Value.Data;
    }

    private static SynapseBigDataPoolInfoData DefaultSparkConfig(string location)
    {
        return new(new AzureLocation(location))
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
            SparkVersion = "3.3",
            DefaultSparkLogFolder = "/logs",
            NodeSize = BigDataPoolNodeSize.Medium,
            NodeSizeFamily = BigDataPoolNodeSizeFamily.MemoryOptimized
        };
    }
}
