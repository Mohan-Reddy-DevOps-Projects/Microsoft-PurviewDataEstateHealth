﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using ProjectBabylon.Metadata;
using System.Threading.Tasks;

/// <summary>
/// Metadata service accessor
/// </summary>
public interface IMetadataAccessorService
{
    /// <summary>
    /// Initialize the service.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Gets a sas token for the processing storage account.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="blobPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StorageTokenKey> GetProcessingStorageSasToken(Guid accountId, string blobPath, CancellationToken cancellationToken);

    Task<StorageTokenKey> GetProcessingStorageDelegationSasToken(
        Guid accountId,
        string containerName,
        string permissions,
        CancellationToken cancellationToken);

    public IProjectBabylonMetadataClient GetMetadataServiceClient();
}
