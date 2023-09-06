// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;

/// <summary>
/// Service to handle storage table and queue operations
/// </summary>
public interface ISingletonStorageAccessorService
{
    /// <summary>
    /// Gets the connection string given the storage account resourceId
    /// </summary>
    /// <param name="resourceId"></param>
    /// <returns></returns>
    Task<string> GetConnectionString(string resourceId);
}
