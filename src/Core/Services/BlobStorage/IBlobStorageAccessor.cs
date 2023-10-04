// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Storage.Blobs;

/// <summary>
/// Provides methods to interact with blob storage.
/// </summary>
public interface IBlobStorageAccessor
{
    /// <summary>
    /// Retrieves the content of a blob as a stream asynchronously.
    /// </summary>
    /// <param name="containerClient">The client for the container containing the blob.</param>
    /// <param name="blobName">The name of the blob to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A stream containing the blob's content.</returns>
    Task<Stream> GetBlobAsync(BlobContainerClient containerClient, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Saves content to a blob asynchronously.
    /// </summary>
    /// <param name="containerClient">The client for the container to save the blob in.</param>
    /// <param name="blobName">The name of the blob to save.</param>
    /// <param name="stream">The stream containing content to save to the blob.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    Task SaveBlobAsync(BlobContainerClient containerClient, string blobName, Stream stream, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a blob asynchronously.
    /// </summary>
    /// <param name="containerClient">The client for the container containing the blob to delete.</param>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    Task DeleteBlobAsync(BlobContainerClient containerClient, string blobName, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a blob container client asynchronously.
    /// </summary>
    /// <param name="blobServiceClient">The blob service client associated with the container.</param>
    /// <param name="containerName">The name of the container to retrieve the client for.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The client for the specified blob container.</returns>
    Task<BlobContainerClient> GetBlobContainerClient(BlobServiceClient blobServiceClient, string containerName, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a blob service client.
    /// </summary>
    /// <param name="storageAccountName">The name of the storage account associated with the blob service.</param>
    /// <param name="storageEndPointSuffix">The storage endpoint suffix for the blob service.</param>
    /// <param name="blobStorageResource">The resource identifier for the blob storage.</param>
    /// <returns>The client for the specified blob service.</returns>
    BlobServiceClient GetBlobServiceClient(string storageAccountName, string storageEndPointSuffix, string blobStorageResource);
}
