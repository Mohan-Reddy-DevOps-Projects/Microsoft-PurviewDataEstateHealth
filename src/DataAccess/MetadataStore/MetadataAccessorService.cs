// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.ProjectBabylon.Metadata;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Rest;
using System;
using System.Net;
using System.Threading.Tasks;

internal class MetadataAccessorService(
    MetadataServiceClientFactory metadataServiceClientFactory,
    IDataEstateHealthRequestLogger logger) : IMetadataAccessorService
{

    /// <inheritdoc/>
    public void Initialize()
    {
        this.GetMetadataServiceClient();
    }

    private IProjectBabylonMetadataClient GetMetadataServiceClient()
    {
        return metadataServiceClientFactory.GetClient();
    }

    /// <inheritdoc/>
    public async Task<StorageTokenKey> GetProcessingStorageSasToken(
        Guid accountId,
        string blobPath,
        CancellationToken cancellationToken)
    {
        StorageSasRequest storageSasRequest = new()
        {
            Services = "b",
            Permissions = "rl",
            BlobPath = blobPath,
            TimeToLive = TimeSpan.FromHours(1).ToString(@"hh\:mm\:ss"),
        };

        try
        {
            IProjectBabylonMetadataClient client = this.GetMetadataServiceClient();
            HttpOperationResponse<StorageTokenKey> response = await client.AccountProcessingStorageSasToken.GetWithHttpMessagesAsync(accountId.ToString(), storageSasRequest, LookupType.ByAccountId, cancellationToken: cancellationToken);

            return response.Body;
        }
        catch (ErrorResponseModelException erx) when (erx.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw this.LogAndConvert(accountId.ToString(), exception);
        }
    }

    /// <inheritdoc/>
    public async Task<StorageTokenKey> GetProcessingStorageDelegationSasToken(
        Guid accountId,
        string containerName,
        string permissions,
        CancellationToken cancellationToken)
    {
        var storageSasRequest = new UserDelegationSasRequest()
        {
            Permissions = permissions,
            TimeToLive = TimeSpan.FromHours(1).ToString(@"hh\:mm\:ss"),
            FileSystemName = containerName
        };

        try
        {
            IProjectBabylonMetadataClient client = this.GetMetadataServiceClient();
            return await client.AccountStorageAccountUserDelegationSasToken.GetAsync(accountId.ToString(), storageSasRequest, LookupType.ByAccountId, null, cancellationToken);
        }
        catch (ErrorResponseModelException erx) when (erx.Response?.StatusCode == HttpStatusCode.NotFound)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw this.LogAndConvert(accountId.ToString(), exception);
        }
    }

    private ServiceException LogAndConvert(
        string accountId,
        Exception exception)
    {
        // Logging as error to avoid multiple criticals for each try. Higher level caller will log critical on failure.
        logger.LogError(
            FormattableString.Invariant(
                $"Failed to perform operation on {accountId}:"),
            exception);

        return new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.MetadataServiceException.Code,
                ErrorMessage.DownstreamDependency)
            .ToException();
    }
}
