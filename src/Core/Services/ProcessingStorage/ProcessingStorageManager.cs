// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure;
using global::Azure.Core;
using global::Azure.ResourceManager.Resources;
using global::Azure.ResourceManager.Storage;
using global::Azure.ResourceManager.Storage.Models;
using global::Azure.Storage.Files.DataLake;
using global::Azure.Storage.Files.DataLake.Models;
using global::Azure.Storage.Sas;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Common;
using System.Threading;
using System.Threading.Tasks;
using DeletionResult = ProjectBabylon.Metadata.Models.DeletionResult;
using DeletionStatus = ProjectBabylon.Metadata.Models.DeletionStatus;
using ProcessingStorageModel = Models.ProcessingStorageModel;
using StorageAccountKey = global::Azure.ResourceManager.Storage.Models.StorageAccountKey;
using StorageSasRequest = Models.StorageSasRequest;

/// <summary>
/// Processing storage manager.
/// </summary>
internal class ProcessingStorageManager : StorageManager<ProcessingStorageConfiguration>, IProcessingStorageManager
{
    private readonly TokenCredential tokenCredential;
    private readonly IStorageAccountRepository<ProcessingStorageModel> storageAccountRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProcessingStorageManager"/> class.
    /// </summary>
    /// <param name="credentialFactory"></param>
    /// <param name="storageAccountRepository"></param>
    /// <param name="azureResourceManagerFactory"></param>
    /// <param name="processingStorageConfiguration"></param>
    /// <param name="logger"></param>
    public ProcessingStorageManager(
        AzureCredentialFactory credentialFactory,
        IStorageAccountRepository<ProcessingStorageModel> storageAccountRepository,
        IAzureResourceManagerFactory azureResourceManagerFactory,
        IOptions<ProcessingStorageConfiguration> processingStorageConfiguration,
        IDataEstateHealthRequestLogger logger) : base(azureResourceManagerFactory.Create<ProcessingStorageAuthConfiguration>(), processingStorageConfiguration, logger)
    {
        this.storageAccountRepository = storageAccountRepository;
        this.tokenCredential = credentialFactory.CreateDefaultAzureCredential();
    }

    /// <inheritdoc/>
    public async Task<ProcessingStorageModel> Get(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        return await this.Get(Guid.Parse(accountServiceModel.Id), cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProcessingStorageModel> Get(Guid accountId, CancellationToken cancellationToken)
    {
        StorageAccountLocator storageAccountKey = new(accountId.ToString(), this.storageConfiguration.StorageNamePrefix);

        return await this.storageAccountRepository.GetSingle(storageAccountKey, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<DeletionResult> Delete(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        StorageAccountLocator storageAccountKey = new(accountServiceModel.Id, this.storageConfiguration.StorageNamePrefix);
        ProcessingStorageModel storageAccountModel = await this.storageAccountRepository.GetSingle(storageAccountKey, cancellationToken);
        if (storageAccountModel == null)
        {
            this.logger.LogInformation($"Storage account not found for {storageAccountKey.Name}");

            return new DeletionResult()
            {
                DeletionStatus = DeletionStatus.ResourceNotFound,
            };
        }

        string resourceId = $"/subscriptions/{this.storageConfiguration.SubscriptionId}";
        SubscriptionResource subscription = this.azureResourceManager.GetSubscription(resourceId);
        ResourceGroupResource resourceGroup = await this.azureResourceManager.GetResourceGroup(subscription, this.storageConfiguration.ResourceGroupName, cancellationToken);
        ResourceIdentifier storageAccountId = new(storageAccountModel.Properties.ResourceId);
        await this.DeleteStorageAccount(resourceGroup, storageAccountId.Name, cancellationToken);
        await this.storageAccountRepository.Delete(storageAccountKey, cancellationToken);
        this.logger.LogInformation($"Successfully deleted storage account {storageAccountModel.Properties.ResourceId}");

        return new DeletionResult()
        {
            DeletionStatus = DeletionStatus.Deleted,
        };
    }

    /// <inheritdoc/>
    public async Task Provision(AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        string resourceId = $"/subscriptions/{this.storageConfiguration.SubscriptionId}";
        SubscriptionResource subscription = this.azureResourceManager.GetSubscription(resourceId);
        ResourceGroupResource resourceGroup = await this.azureResourceManager.GetResourceGroup(subscription, this.storageConfiguration.ResourceGroupName, cancellationToken);

        StorageAccountRequestModel processingStorageModelRequest = new()
        {
            Location = accountServiceModel.Location,
            ResourceGroup = this.storageConfiguration.ResourceGroupName,
            SubscriptionId = this.storageConfiguration.SubscriptionId,
            TenantId = this.storageConfiguration.TenantId,
        };
        ProcessingStorageModel existingStorageModel = await this.Get(accountServiceModel, cancellationToken);
        UpsertStorageResource upsertStorage = await this.CreateOrUpdateStorageResource(processingStorageModelRequest, subscription, resourceGroup, existingStorageModel, cancellationToken);
        ProcessingStorageModel newStorageModel = this.ToModel(upsertStorage.StorageAccount, accountServiceModel, existingStorageModel);

        // only persist the model definition if the storage account is successfully created along with all the other resources
        if (upsertStorage.Update)
        {
            await this.storageAccountRepository.Update(newStorageModel, accountServiceModel.Id, cancellationToken);
        }
        else
        {
            await this.CreateManagementPolicy(upsertStorage.StorageAccount, cancellationToken);
            await this.CreateDefaultContainer(upsertStorage.StorageAccount, accountServiceModel, cancellationToken);
            await this.storageAccountRepository.Create(newStorageModel, accountServiceModel.Id, cancellationToken);
        }
        this.logger.LogInformation($"Successfully created storage account {newStorageModel.Properties.ResourceId}");
    }

    /// <inheritdoc/>
    public async Task<Uri> GetProcessingStorageSasUri(ProcessingStorageModel processingStorageModel, StorageSasRequest parameters, string containerName, CancellationToken cancellationToken)
    {
        string serviceEndpoint = processingStorageModel.GetDfsEndpoint();
        DataLakeServiceClient serviceClient = new(new Uri(serviceEndpoint), this.tokenCredential);
        DataLakeFileSystemClient fileSystemClient = new(new Uri($"{serviceEndpoint}/{containerName}"), this.tokenCredential);
        DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient(parameters.Path);

        return await GetUserDelegationSasDirectory(directoryClient, serviceClient, parameters);
    }

    /// <inheritdoc/>
    public async Task<string> ConstructContainerPath(string containerName, Guid accountId, CancellationToken cancellationToken)
    {
        ProcessingStorageModel storageModel = await this.Get(accountId, cancellationToken);

        ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));

        return $"{storageModel.GetDfsEndpoint()}/{containerName}";
    }

    public async Task<Stream> GetDataQualityOutput(
        ProcessingStorageModel processingStorageModel,
        string folderPath,
        string fileName)
    {
        string serviceEndpoint = processingStorageModel.GetDfsEndpoint();
        var serviceClient = new DataLakeServiceClient(new Uri(serviceEndpoint), this.tokenCredential);
        var fileSystemClient = serviceClient.GetFileSystemClient(processingStorageModel.CatalogId.ToString()); ;
        var directoryClient = fileSystemClient.GetDirectoryClient(folderPath);
        var fileClient = directoryClient.GetFileClient(fileName);

        var fileResponse = await fileClient.ReadAsync().ConfigureAwait(false);
        return fileResponse.Value.Content;
    }

    /// <summary>
    /// Get a user delegation Sas URI to the data lake directory.
    /// </summary>
    /// <param name="directoryClient"></param>
    /// <param name="dataLakeServiceClient"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    private async static Task<Uri> GetUserDelegationSasDirectory(DataLakeDirectoryClient directoryClient, DataLakeServiceClient dataLakeServiceClient, StorageSasRequest parameters)
    {
        // Get a user delegation key that's valid for seven days.
        // Use the key to generate any number of shared access signatures over the lifetime of the key.
        UserDelegationKey userDelegationKey = await dataLakeServiceClient.GetUserDelegationKeyAsync(DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(5)), DateTimeOffset.UtcNow.AddDays(7));

        // Create a SAS token
        DataLakeSasBuilder sasBuilder = new()
        {
            FileSystemName = directoryClient.FileSystemName,
            Resource = "d",
            IsDirectory = true,
            Path = parameters.Path,
            Protocol = SasProtocol.Https,
            ExpiresOn = DateTimeOffset.UtcNow.Add(parameters.TimeToLive)
        };

        sasBuilder.SetPermissions(parameters.Permissions);

        DataLakeUriBuilder fullUri = new(directoryClient.Uri)
        {
            Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, dataLakeServiceClient.AccountName)
        };

        return fullUri.ToUri();
    }

    private async Task<string> GetStorageAccountKey(StorageAccountResource storageAccount, CancellationToken cancellationToken)
    {
        AsyncPageable<StorageAccountKey> keys = storageAccount.GetKeysAsync(cancellationToken: cancellationToken);
        List<StorageAccountKey> response = new();
        await foreach (StorageAccountKey key in keys)
        {
            response.Add(key);
        }

        return response.First(x => x.KeyName.Equals("key2", StringComparison.OrdinalIgnoreCase)).Value;
    }

    /// <summary>
    /// Create container.
    /// </summary>
    /// <param name="storageAccount"></param>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task CreateDefaultContainer(StorageAccountResource storageAccount, AccountServiceModel accountServiceModel, CancellationToken cancellationToken)
    {
        await this.CreateContainer(storageAccount, accountServiceModel.DefaultCatalogId, cancellationToken);
    }

    /// <summary>
    /// Will call the storage SKU API to determine if storage supports zone redundant storage for the subscription and location. The result is cached in memory.
    /// </summary>
    /// <param name="subscription"></param>
    /// <param name="location"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Whether the location and subscription support zone redundant storage.</returns>
    private async Task<bool> IsStorageZRSAvailable(
        SubscriptionResource subscription,
        string location,
        CancellationToken cancellationToken)
    {
        AsyncPageable<StorageSkuInformation> skus = subscription.GetSkusAsync(cancellationToken);

        await foreach (StorageSkuInformation sku in skus)
        {
            if (sku.Name == StorageSkuName.StandardZrs &&
                sku.Tier == StorageSkuTier.Standard &&
                sku.Kind == Management.Storage.Models.Kind.StorageV2 &&
                sku.Locations.Any(l => l.Equals(location, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Convert to the processing storage model.
    /// </summary>
    /// <param name="storageAccount"></param>
    /// <param name="accountServiceModel"></param>
    /// <param name="existingStorageModel"></param>
    /// <returns></returns>
    private ProcessingStorageModel ToModel(StorageAccountResource storageAccount, AccountServiceModel accountServiceModel, ProcessingStorageModel existingStorageModel)
    {
        bool isPartitionDnsEnabled = storageAccount.Data.DnsEndpointType == DnsEndpointType.AzureDnsZone;
        Uri endpoint = storageAccount.Data.PrimaryEndpoints.BlobUri;
        string dnsZone = isPartitionDnsEnabled ? StorageUtilities.GetStorageDnsZoneFromEndpoint(endpoint) : string.Empty;
        string endpointSuffix = StorageUtilities.GetStorageEndpointSuffix(endpoint, isPartitionDnsEnabled);
        DateTime now = DateTime.UtcNow;

        return new()
        {
            AccountId = Guid.Parse(accountServiceModel.Id),
            Id = existingStorageModel?.Id ?? Guid.NewGuid(),
            Name = this.storageConfiguration.StorageNamePrefix,
            TenantId = Guid.Parse(accountServiceModel.TenantId),
            CatalogId = Guid.Parse(accountServiceModel.DefaultCatalogId),
            Properties = new()
            {
                CreatedAt = existingStorageModel?.Properties?.CreatedAt ?? now,
                DnsZone = dnsZone,
                EndpointSuffix = endpointSuffix,
                LastModifiedAt = now,
                Location = accountServiceModel.Location,
                ResourceId = storageAccount.Id,
                Sku = storageAccount.Data.Sku.Name.ToString()
            },
        };
    }

    private async Task<UpsertStorageResource> CreateOrUpdateStorageResource(
        StorageAccountRequestModel storageAccountRequest,
        SubscriptionResource subscription,
        ResourceGroupResource resourceGroup,
        ProcessingStorageModel existingStorageModel,
        CancellationToken cancellationToken)
    {
        StorageAccountResource storageAccount;
        if (existingStorageModel == null)
        {
            bool isZoneRedundant = await this.IsStorageZRSAvailable(subscription, storageAccountRequest.Location, cancellationToken);
            string skuName = isZoneRedundant ? StorageSkuName.StandardZrs.ToString() : StorageSkuName.StandardLrs.ToString();
            storageAccountRequest.Sku = skuName;
            storageAccount = await this.CreateStorageAccount(subscription, resourceGroup, storageAccountRequest, cancellationToken);
        }
        else
        {
            ResourceIdentifier storageId = new(existingStorageModel.Properties.ResourceId);
            storageAccountRequest.Name = storageId.Name;
            storageAccountRequest.Sku = existingStorageModel.Properties.Sku;
            storageAccount = await this.UpdateStorageAccount(resourceGroup, storageAccountRequest, cancellationToken);
        }

        return (storageAccount, existingStorageModel != null);
    }

    private record struct UpsertStorageResource(StorageAccountResource StorageAccount, bool Update)
    {
        public static implicit operator UpsertStorageResource((StorageAccountResource, bool) value) => new(value.Item1, value.Item2);
    }
}
