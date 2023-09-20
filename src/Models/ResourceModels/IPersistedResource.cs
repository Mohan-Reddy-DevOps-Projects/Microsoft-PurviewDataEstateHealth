// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Generic type with properties that each share resource will have and be persisted to the persistence store.
/// </summary>
public interface IPersistedResource<out TProperties> 
{
    /// <summary>
    /// Property bag of the resource to be persisted to the persistence store.
    /// </summary>
    TProperties Properties { get; }
}
