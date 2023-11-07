// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Base repository interface
/// </summary>
public interface IBaseRepository 
{
    /// <summary>
    /// Constructs storage account container path for serverless queries.
    /// </summary>
    /// <param name="containerName"> The container name.</param>
    /// <param name="accountId">Account Id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public Task<string> ConstructContainerPath(string containerName, Guid accountId, CancellationToken cancellationToken);
}
