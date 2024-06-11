// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure;
using global::Azure.Core;
using global::Azure.ResourceManager;
using global::Azure.ResourceManager.Synapse;
using System;
using System.Threading.Tasks;

internal sealed partial class AzureResourceManager<TAuthConfig>
{
    public async Task<SynapseBigDataPoolInfoData> CreateOrUpdateSparkPool(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, SynapseBigDataPoolInfoData sparkConfig, CancellationToken cancellationToken)
    {
        SynapseBigDataPoolInfoCollection collection = this.GetSynapseWorkspace(subscriptionId, resourceGroupName, workspaceName);
        ArmOperation<SynapseBigDataPoolInfoResource> lro = await collection.CreateOrUpdateAsync(WaitUntil.Completed, bigDataPoolName, sparkConfig, cancellationToken: cancellationToken);

        return lro.Value.Data;
    }

    public async Task<SynapseBigDataPoolInfoData> GetSparkPool(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, CancellationToken cancellationToken)
    {
        SynapseBigDataPoolInfoCollection collection = this.GetSynapseWorkspace(subscriptionId, resourceGroupName, workspaceName);
        Response<SynapseBigDataPoolInfoResource> lro = await collection.GetAsync(bigDataPoolName, cancellationToken);

        return lro.Value.Data;
    }

    public async Task<List<SynapseBigDataPoolInfoData>> ListSparkPools(Guid subscriptionId, string resourceGroupName, string workspaceName, CancellationToken cancellationToken)
    {
        SynapseBigDataPoolInfoCollection collection = this.GetSynapseWorkspace(subscriptionId, resourceGroupName, workspaceName);
        AsyncPageable<SynapseBigDataPoolInfoResource> resp = collection.GetAllAsync(cancellationToken);

        List<SynapseBigDataPoolInfoData> result = new List<SynapseBigDataPoolInfoData>();
        await foreach (var page in resp.AsPages())
        {
            result.AddRange(page.Values.Select(v => v.Data));
        }

        return result;
    }

    public async Task DeleteSparkPool(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, CancellationToken cancellationToken)
    {
        SynapseBigDataPoolInfoCollection collection = this.GetSynapseWorkspace(subscriptionId, resourceGroupName, workspaceName);
        Response<SynapseBigDataPoolInfoResource> lro = await collection.GetAsync(bigDataPoolName, cancellationToken);

        await lro.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);
    }

    public async Task<bool> SparkPoolExists(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, CancellationToken cancellationToken)
    {
        SynapseBigDataPoolInfoCollection collection = this.GetSynapseWorkspace(subscriptionId, resourceGroupName, workspaceName);
        Response<bool> response = await collection.ExistsAsync(bigDataPoolName, cancellationToken);

        return response.Value;
    }

    private SynapseBigDataPoolInfoCollection GetSynapseWorkspace(Guid subscriptionId, string resourceGroupName, string workspaceName)
    {
        ResourceIdentifier synapseWorkspaceResourceId = SynapseWorkspaceResource.CreateResourceIdentifier(subscriptionId.ToString(), resourceGroupName, workspaceName);
        SynapseWorkspaceResource synapseWorkspace = this.armClient.GetSynapseWorkspaceResource(synapseWorkspaceResourceId);

        return synapseWorkspace.GetSynapseBigDataPoolInfos();
    }
}
