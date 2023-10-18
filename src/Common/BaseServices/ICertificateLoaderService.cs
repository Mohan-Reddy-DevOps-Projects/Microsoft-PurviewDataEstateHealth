// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Interface to provide certificates.
/// </summary>
public interface ICertificateLoaderService
{
    /// <summary>
    /// Load certificate
    /// </summary>
    /// <returns>Certificate loaded from the configured store</returns>
    Task<X509Certificate2> LoadAsync(string secretName, CancellationToken cancellationToken);

    /// <summary>
    /// Binds a cert to a message handler
    /// </summary>
    /// <param name="httpMessageHandler"></param>
    /// <param name="certName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    Task BindAsync(HttpMessageHandler httpMessageHandler, string certName, CancellationToken cancellationToken);

    /// <summary>
    /// Initializes the cache.
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync();
}
