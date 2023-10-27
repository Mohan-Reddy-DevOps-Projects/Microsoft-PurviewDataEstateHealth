// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

using System;
using System.Threading.Tasks;
using global::Azure.ResourceManager.Resources;
using global::Azure.ResourceManager.Storage.Models;
using global::Azure.ResourceManager.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Processing storage manager.
/// </summary>
internal class ProcessingStorageManager : IProcessingStorageManager
{
    private readonly IAzureResourceManager azureResourceManager;
    private readonly ProcessingStorageConfiguration processingStorageConfiguration;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessingStorageManager"/> class.
    /// </summary>
    /// <param name="azureResourceManager"></param>
    /// <param name="processingStorageConfiguration"></param>
    public ProcessingStorageManager(IAzureResourceManager azureResourceManager, IOptions<ProcessingStorageConfiguration> processingStorageConfiguration)
    {
        this.azureResourceManager = azureResourceManager;
        this.processingStorageConfiguration = processingStorageConfiguration.Value;
    }

    /// <inheritdoc/>
    public async Task Provision(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        string resourceId = this.processingStorageConfiguration.ResourceId;
        string resourceGroupName = this.processingStorageConfiguration.ResourceGroupName;
        string location = this.processingStorageConfiguration.AzureRegion;
        string accountName = $"dgprocessing{Guid.NewGuid().ToString("N")[..12]}";
        await this.ProvisionStorage(resourceId, resourceGroupName, accountName, location, accountServiceModel.DefaultCatalogId, cancellationToken);
    }

    /// <summary>
    /// Creates an ADLS Gen2 Storage Account in Private DNS Zone.
    /// </summary>
    /// <param name="resourceId"></param>
    /// <param name="resourceGroupName"></param>
    /// <param name="accountName"></param>
    /// <param name="location"></param>
    /// <param name="blobContainerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ProvisionStorage(string resourceId, string resourceGroupName, string accountName, string location, string blobContainerName, CancellationToken cancellationToken)
    {
        StorageSku sku = new(StorageSkuName.StandardLrs);
        StorageKind kind = StorageKind.StorageV2;
        StorageAccountCreateOrUpdateContent parameters = new(sku, kind, location)
        {
            DnsEndpointType = DnsEndpointType.AzureDnsZone,
        };
        SubscriptionResource subscription = this.azureResourceManager.GetSubscription(resourceId);
        ResourceGroupResource resourceGroup = await this.azureResourceManager.GetResourceGroup(subscription, resourceGroupName, cancellationToken);
        // Storage account has to be globally unique and inside private dns zone
        StorageAccountResource storageAccount = await this.azureResourceManager.CreateOrUpdateStorageAccount(resourceGroup, accountName, parameters, cancellationToken);
        await this.azureResourceManager.CreateStorageContainer(storageAccount, blobContainerName, cancellationToken);
    }
}
