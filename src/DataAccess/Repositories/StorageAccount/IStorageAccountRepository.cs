// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Storage account repository interface
/// </summary>
public interface IStorageAccountRepository<T> : IGetSingleOperation<T, StorageAccountLocator>
{
    /// <summary>
    /// Creates the storage account
    /// </summary>
    /// <param name="model"></param>
    /// <param name="partitionKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<T> Create(T model, string partitionKey, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the storage account
    /// </summary>
    /// <param name="entityLocator"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Delete(StorageAccountLocator entityLocator, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the storage account
    /// </summary>
    /// <param name="model"></param>
    /// <param name="partitionKey"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Update(T model, string partitionKey, CancellationToken cancellationToken);
}
