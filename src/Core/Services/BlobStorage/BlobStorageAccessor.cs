// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using global::Azure;
using global::Azure.Core;
using global::Azure.Storage.Blobs;
using global::Azure.Storage.Blobs.Models;
using global::Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json.Linq;

/// <summary>
/// Defines the <see cref="BlobStorageAccessor" />.
/// </summary>
internal sealed class BlobStorageAccessor : IBlobStorageAccessor
{
    private const string Tag = nameof(BlobStorageAccessor);
    private readonly AadAppTokenProviderService<FirstPartyAadAppConfiguration> tokenProvider;
    private readonly IDataEstateHealthLogger logger;
    private readonly AuxStorageConfiguration storageConfiguration;

    private static readonly BlobClientOptions options = new()
    {
        Retry =
        {
            Mode = RetryMode.Exponential,
            MaxRetries = 3,
            Delay = TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
        },
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageAccessor"/> class.
    /// </summary>
    /// <param name="tokenProvider"></param>
    /// <param name="logger">logger</param>
    /// <param name="storageConfiguration">Storage configuration</param>
    public BlobStorageAccessor(AadAppTokenProviderService<FirstPartyAadAppConfiguration> tokenProvider, IDataEstateHealthLogger logger, IOptions<AuxStorageConfiguration> storageConfiguration)
    {
        this.tokenProvider = tokenProvider;
        this.logger = logger;
        this.storageConfiguration = storageConfiguration.Value;
    }    

    /// <inheritdoc/>
    public async Task<Stream> GetBlobAsync(BlobContainerClient containerClient, string blobName, CancellationToken cancellationToken)
    {
        try
        {
            AppendBlobClient blobClient = GetAppendBlobClient(containerClient, blobName);
            if (await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
            {
                BlobDownloadInfo download = await blobClient.DownloadAsync(cancellationToken);
                this.logger.LogInformation($"{Tag}|Blob {containerClient.Name}/{blobName} downloaded successfully");
                return download.Content;
            }
            else
            {
                return null;
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
        {
            this.logger.LogWarning($"{Tag}|Blob not found: {containerClient.Name}/{blobName}");
            return null;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{Tag}|An error occurred while getting blob {containerClient.Name}/{blobName}", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveBlobAsync(BlobContainerClient containerClient, string blobName, Stream stream, CancellationToken cancellationToken = default)
    {
        try
        {
            AppendBlobClient blobClient = GetAppendBlobClient(containerClient, blobName);

            await blobClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            await blobClient.AppendBlockAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
            this.logger.LogWarning($"{Tag}|Blob {containerClient.Name}/{blobName} saved successfully");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{Tag}|An error occurred while saving blob {containerClient.Name}/{blobName}", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteBlobAsync(BlobContainerClient containerClient, string blobName, CancellationToken cancellationToken)
    {
        try
        {
            AppendBlobClient blobClient = GetAppendBlobClient(containerClient, blobName);

            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
            this.logger.LogInformation($"{Tag}|Blob {containerClient.Name}/{blobName} deleted successfully");
        }
        catch (Exception ex)
        {
            this.logger.LogError($"{Tag}|An error occurred while deleting blob {containerClient.Name}/{blobName}", ex);
            throw;
        }
    }

    /// <inheritdoc/>
    public BlobServiceClient GetBlobServiceClient(string storageAccountName, string storageEndPointSuffix, string blobStorageResource)
    {
        TokenCredential tokenCredential = new AuthTokenCredential<FirstPartyAadAppConfiguration>(this.tokenProvider, blobStorageResource);

        return new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.{storageEndPointSuffix}"), tokenCredential, options);
    }

    /// <inheritdoc/>
    public async Task<BlobContainerClient> GetBlobContainerClient(BlobServiceClient blobServiceClient, string containerName, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();

        return containerClient;
    }

    private static AppendBlobClient GetAppendBlobClient(BlobContainerClient containerClient, string blobName)
    {
        return containerClient.GetAppendBlobClient(blobName);
    }
}
