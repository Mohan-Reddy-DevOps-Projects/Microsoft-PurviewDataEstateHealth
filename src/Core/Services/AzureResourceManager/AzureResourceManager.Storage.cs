// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using global::Azure.ResourceManager.Resources;
using global::Azure.ResourceManager.Storage.Models;
using global::Azure.ResourceManager.Storage;
using global::Azure.ResourceManager;
using global::Azure;

internal sealed partial class AzureResourceManager<TAuthConfig>
{
    /// <inheritdoc/>
    public async Task<StorageAccountResource> CreateOrUpdateStorageAccount(ResourceGroupResource resourceGroup, string accountName, StorageAccountCreateOrUpdateContent parameters, CancellationToken cancellationToken)
    {
        StorageAccountCollection accountCollection = resourceGroup.GetStorageAccounts();
        ArmOperation<StorageAccountResource> accountCreateOperation = await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, accountName, parameters, cancellationToken);
        StorageAccountResource storageAccount = accountCreateOperation.Value;

        return storageAccount;
    }

    /// <inheritdoc/>
    public async Task<BlobContainerResource> CreateStorageContainer(StorageAccountResource storageAccount, string blobContainerName, CancellationToken cancellationToken)
    {
        BlobServiceResource blobService = storageAccount.GetBlobService();
        BlobContainerCollection blobContainerCollection = blobService.GetBlobContainers();
        BlobContainerData blobContainerData = new();
        ArmOperation<BlobContainerResource> blobContainerCreateOperation = await blobContainerCollection.CreateOrUpdateAsync(WaitUntil.Completed, blobContainerName, blobContainerData, cancellationToken);
        BlobContainerResource blobContainer = blobContainerCreateOperation.Value;

        return blobContainer;
    }

    public async Task<StorageAccountManagementPolicyResource> CreateOrUpdateStorageManagementPolicy(StorageAccountResource storageAccount, StorageAccountManagementPolicyData managementPolicyData, CancellationToken cancellationToken)
    {
        StorageAccountManagementPolicyResource managementPolicyResource = storageAccount.GetStorageAccountManagementPolicy();
        ArmOperation<StorageAccountManagementPolicyResource> response = await managementPolicyResource.CreateOrUpdateAsync(WaitUntil.Completed, managementPolicyData, cancellationToken);

        return response.Value;
    }
}
