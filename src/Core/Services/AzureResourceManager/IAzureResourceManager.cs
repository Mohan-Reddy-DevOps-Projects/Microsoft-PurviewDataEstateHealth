// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using global::Azure.ResourceManager.Resources;
using global::Azure.ResourceManager.Storage;
using global::Azure.ResourceManager.Storage.Models;
using global::Azure.ResourceManager.Synapse;

/// <summary>
/// Interface for Azure Resource Manager.
/// </summary>
public interface IAzureResourceManager
{
    /// <summary>
    /// Gets a resource group.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="resourceGroupName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ResourceGroupResource> GetResourceGroup(SubscriptionResource subscription, string resourceGroupName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a subscription.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <returns></returns>
    SubscriptionResource GetSubscription(string resourceId);

    /// <summary>
    /// Asynchronously creates a new storage account with the specified parameters.
    /// If an account is already created and a subsequent create request is issued with different properties, the account properties will be updated.
    /// If an account is already created and a subsequent create or update request is issued with the exact same set of properties, the request will succeed.
    /// </summary>
    /// <param name="resourceGroup"></param>
    /// <param name="accountName"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageAccountResource> CreateOrUpdateStorageAccount(ResourceGroupResource resourceGroup, string accountName, StorageAccountCreateOrUpdateContent parameters, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new container under the specified account as described by request body.
    /// The container resource includes metadata and properties for that container.
    /// It does not include a list of the blobs contained by the container. 
    /// </summary>
    /// <param name="storageAccount"></param>
    /// <param name="blobContainerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<BlobContainerResource> CreateStorageContainer(StorageAccountResource storageAccount, string blobContainerName, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new management policy for the specified storage account.
    /// </summary>
    /// <param name="storageAccount"></param>
    /// <param name="managementPolicyData"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageAccountManagementPolicyResource> CreateOrUpdateStorageManagementPolicy(StorageAccountResource storageAccount, StorageAccountManagementPolicyData managementPolicyData, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new Synapse spark pool.
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="resourceGroupName"></param>
    /// <param name="workspaceName"></param>
    /// <param name="bigDataPoolName"></param>
    /// <param name="location"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SynapseBigDataPoolInfoData> CreateOrUpdateSparkPool(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, string location, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the Synapse spark pool.
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="resourceGroupName"></param>
    /// <param name="workspaceName"></param>
    /// <param name="bigDataPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SynapseBigDataPoolInfoData> GetSparkPool(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, CancellationToken cancellationToken);

    /// <summary>
    /// Whether the Synapse spark pool exists or not.
    /// </summary>
    /// <param name="subscriptionId"></param>
    /// <param name="resourceGroupName"></param>
    /// <param name="workspaceName"></param>
    /// <param name="bigDataPoolName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<bool> SparkPoolExists(Guid subscriptionId, string resourceGroupName, string workspaceName, string bigDataPoolName, CancellationToken cancellationToken);
}
