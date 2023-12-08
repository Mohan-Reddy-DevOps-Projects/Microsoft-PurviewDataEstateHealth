// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using global::Azure;
using global::Azure.ResourceManager.Resources;
using global::Azure.ResourceManager.Storage.Models;
using global::Azure.ResourceManager.Storage;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal abstract class StorageManager<TConfig>
    where TConfig : ProcessingStorageConfiguration
{
    protected readonly IDataEstateHealthRequestLogger logger;
    protected readonly IAzureResourceManager azureResourceManager;
    protected readonly TConfig storageConfiguration;

    public StorageManager(
        IAzureResourceManager azureResourceManager,
        IOptions<TConfig> storageConfiguration,
        IDataEstateHealthRequestLogger logger)
    {
        this.logger = logger;
        this.storageConfiguration = storageConfiguration.Value;
        this.azureResourceManager = azureResourceManager;
    }

    protected async Task DeleteStorageAccount(ResourceGroupResource resourceGroup, string accountName, CancellationToken cancellationToken)
    {
        Response<StorageAccountResource> response = await resourceGroup.GetStorageAccountAsync(accountName, StorageAccountExpand.BlobRestoreStatus, cancellationToken);
        StorageAccountResource storageAccount = response.Value;

        await storageAccount.DeleteAsync(WaitUntil.Completed, cancellationToken);
    }

    protected async Task<StorageAccountResource> UpdateStorageAccount(ResourceGroupResource resourceGroup, StorageAccountRequestModel storageModel, CancellationToken cancellationToken)
    {
        StorageAccountCreateOrUpdateContent parameters = CreateStorageRequest(storageModel);
        return await this.CreateOrUpdateStorageAccount(resourceGroup, storageModel.Name, parameters, cancellationToken);
    }

    protected async Task<StorageAccountResource> CreateStorageAccount(SubscriptionResource subscription, ResourceGroupResource resourceGroup, StorageAccountRequestModel model, CancellationToken cancellationToken)
    {
        string prefix = $"{this.storageConfiguration.StorageNamePrefix}".ToLowerInvariant();
        string storageAccountName = await this.GenerateStorageAccountName(subscription, prefix);
        model.Name = storageAccountName;

        StorageAccountCreateOrUpdateContent parameters = CreateStorageRequest(model);
        return await this.CreateOrUpdateStorageAccount(resourceGroup, model.Name, parameters, cancellationToken);
    }

    protected async Task CreateManagementPolicy(StorageAccountResource storageAccount, CancellationToken cancellationToken)
    {
        await this.azureResourceManager.CreateOrUpdateStorageManagementPolicy(
        storageAccount,
        ManagementPolicies.GetStorageManagementPolicyRules(), cancellationToken);
        this.logger.LogInformation($"Successfully created storage management policies for {storageAccount.Data.Id}");
    }

    protected async Task CreateContainer(StorageAccountResource storageAccount, string containerName, CancellationToken cancellationToken)
    {
        BlobContainerResource container = await this.azureResourceManager.CreateStorageContainer(storageAccount, containerName, cancellationToken);
        this.logger.LogInformation($"Successfully created storage container {container.Data.Id} {container.Data.Name}");
    }

    /// <summary>
    /// Creates or updates a storage account.
    /// </summary>
    /// <param name="resourceGroup"></param>
    /// <param name="storageAccountName"></param>
    /// <param name="parameters"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<StorageAccountResource> CreateOrUpdateStorageAccount(ResourceGroupResource resourceGroup, string storageAccountName, StorageAccountCreateOrUpdateContent parameters, CancellationToken cancellationToken)
    {
        this.logger.LogInformation($"Creating or updating storage account {storageAccountName}");
        StorageAccountResource response = await this.azureResourceManager.CreateOrUpdateStorageAccount(resourceGroup, storageAccountName, parameters, cancellationToken);

        return response;
    }

    /// <summary>
    /// Generates a valid storage account name that is available.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="prefix"></param>
    /// <returns></returns>
    private async Task<string> GenerateStorageAccountName(SubscriptionResource subscription, string prefix)
    {
        const int maxAttempts = 3;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            string storageAccountName = StorageUtilities.ConstructStorageAccountName(prefix);
            this.logger.LogInformation($"Checking name availability for storage account {storageAccountName} attempt {attempt}");

            if (await CheckStorageAccountNameAvailability(subscription, storageAccountName))
            {
                return storageAccountName;
            }
        }

        throw new ServiceError(
            ErrorCategory.ServiceError,
            ErrorCode.Unknown.Code,
            $"The storage account name is not available.").ToException();
    }

    /// <summary>
    /// Checks that the storage account name is valid and is not already in use.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="storageAccountName"></param>
    /// <returns></returns>
    private static async Task<bool> CheckStorageAccountNameAvailability(
        SubscriptionResource subscription,
        string storageAccountName)
    {
        Response<StorageAccountNameAvailabilityResult> response = await subscription.CheckStorageAccountNameAvailabilityAsync(new StorageAccountNameAvailabilityContent(storageAccountName));

        return response.Value.IsNameAvailable ?? false;
    }

    /// <summary>
    /// Creates a storage account request for and ADLS Gen2 Storage Account in Private DNS Zone.
    /// </summary>
    /// <param name="storageModel"></param>
    /// <returns></returns>
    private static StorageAccountCreateOrUpdateContent CreateStorageRequest(StorageAccountRequestModel storageModel)
    {
        StorageSku sku = new(storageModel.Sku);
        StorageKind kind = StorageKind.StorageV2;
        return new(sku, kind, storageModel.Location)
        {
            AllowBlobPublicAccess = false,
            AllowCrossTenantReplication = false,
            DnsEndpointType = Management.Storage.Models.DnsEndpointType.AzureDnsZone,
            IsHnsEnabled = true,
            MinimumTlsVersion = StorageMinimumTlsVersion.Tls1_2,
            PublicNetworkAccess = StoragePublicNetworkAccess.Enabled,
        };
    }
}
