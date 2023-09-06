// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Rest.TransientFaultHandling;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Rest;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.DGP.ServiceBasics.Errors;

/// <inheritdoc />
public class SingletonStorageAccessorService : ISingletonStorageAccessorService
{
    private readonly EnvironmentConfiguration environmentConfiguration;

    private readonly RetryPolicy<HttpStatusCodeErrorDetectionStrategy> retryPolicy =
        new RetryPolicy<HttpStatusCodeErrorDetectionStrategy>(
            new FixedIntervalRetryStrategy(5, TimeSpan.FromSeconds(15)));

    private readonly IDataEstateHealthLogger dataEstateHealthLogger;

    private readonly IMemoryCache cache;

    /// <summary>
    /// Instantiate instance of SingletonStorageAccessorService.
    /// </summary>
    public SingletonStorageAccessorService(
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        IDataEstateHealthLogger dataEstateHealthLogger,
        IMemoryCache cache)
    {
        this.environmentConfiguration = environmentConfiguration.Value;
        this.dataEstateHealthLogger = dataEstateHealthLogger;
        this.cache = cache;
    }

    /// <inheritdoc />
    public async Task<string> GetConnectionString(string resourceId)
    {
        //REVIST IMPLEMENTATION : add trace async method for retry and logging
        if (resourceId.Length == 0)
        {
            throw new Exception("ResourceId cannot be empty");
        }

        if (!this.cache.TryGetValue(resourceId, out string cachedConnectionString))
        {
            // Return value for localhost
            if (this.environmentConfiguration.IsDevelopmentEnvironment())
            {
                string localConnString = "UseDevelopmentStorage=true";
                this.cache.Set(resourceId, localConnString, TimeSpan.FromHours(1));

                return localConnString;
            }

            string cacheBackupKey = $"{resourceId}-backup";
            IList<StorageAccountKey> accessKeys;
            var storageResourceId = ResourceId.FromString(resourceId);

            try
            {
                ServiceClientCredentials credentials = await this.GetServiceCredentialsAsync();

                var storageManagementClient = new StorageManagementClient(credentials)
                {
                    SubscriptionId = storageResourceId.SubscriptionId
                };

                accessKeys = (await storageManagementClient.StorageAccounts.ListKeysAsync(
                    storageResourceId.ResourceGroupName,
                    storageResourceId.Name)).Keys;

                if (accessKeys.Count == 0)
                {
                    throw new ServiceError(
                            ErrorCategory.DownStreamError,
                            ErrorCode.Storage_FailedToGetAccessKeys.Code,
                            FormattableString.Invariant(
                                $"No access keys were found for storage account with resource id {resourceId}."))
                        .ToException();
                }
            }
            catch (Exception exception)
            {
                this.dataEstateHealthLogger.LogError(
                    $"Unable to retrieve the connection string for the storage account with resource id: {resourceId}, attempting to use backup...",
                    exception);

                if (!this.cache.TryGetValue(cacheBackupKey, out string backupConnectionString))
                {
                    this.dataEstateHealthLogger.LogCritical(
                        $"Unable to retrieve the connection string for the storage account with resource id: {resourceId}",
                        exception);

                    throw new ServiceError(
                            ErrorCategory.DownStreamError,
                            ErrorCode.Storage_FailedToGetAccessKeys.Code,
                            FormattableString.Invariant(
                                $"Failed to retrieve access keys for storage account with resource id {resourceId}"))
                        .ToException();
                }

                this.cache.Set(resourceId, backupConnectionString, TimeSpan.FromHours(1));
                this.dataEstateHealthLogger.LogInformation(
                    $"Fetched backup connection string for storage account with resource id: {resourceId}");

                return backupConnectionString;
            }

            cachedConnectionString = this.GenerateConnectionString(
                accessKeys.First().Value,
                storageResourceId);
            this.cache.Set(resourceId, cachedConnectionString, TimeSpan.FromHours(1));
            this.cache.Set(cacheBackupKey, cachedConnectionString);
        }

        return cachedConnectionString;
    }

    private string GenerateConnectionString(string accessKey, ResourceId storageResourceId)
    {
        return
            $"DefaultEndpointsProtocol=https;AccountName={storageResourceId.Name};AccountKey={accessKey};EndpointSuffix={this.environmentConfiguration.AzureEnvironment.StorageEndpointSuffix}";
    }

    private async Task<ServiceClientCredentials> GetServiceCredentialsAsync()
    {
        var azureServiceTokenProvider = new AzureServiceTokenProvider();

        string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync(
            this.environmentConfiguration.AzureEnvironment.ManagementEndpoint);

        return new TokenCredentials(accessToken);
    }
}
