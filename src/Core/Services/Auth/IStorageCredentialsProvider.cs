// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

/// <summary>
/// A provider of credentials for a <see cref="CloudStorageAccount"/>.
/// </summary>
public interface IStorageCredentialsProvider
{
    /// <summary>
    /// Retrieve the credentials for a <see cref="CloudStorageAccount"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> that returns a set of <see cref="StorageCredentials"/>.</returns>
    Task<StorageCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieve renewable credentials for a <see cref="CloudStorageAccount"/>.
    /// </summary>
    /// <returns>A <see cref="Task"/> that returns a set of <see cref="StorageCredentials"/>.</returns>
    Task<StorageCredentials> GetRenewableCredentialsAsync(CancellationToken cancellationToken = default);
}
