// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Enumerates different operation types
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum OperationType
{
    /// <summary>
    /// Resource to create or update
    /// </summary>
    CreateOrUpdate = 1,

    /// <summary>
    /// Resource to delete 
    /// </summary>
    Delete,

    /// <summary>
    /// Resource to soft delete
    /// </summary>
    SoftDelete
}
