// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

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
    /// Gets a managed identity token.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> GetManagedIdentityTokensAsync(
            string accountId,
            CancellationToken cancellationToken);
}
