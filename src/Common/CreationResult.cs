// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// A type that refers to the creation of a resource.
/// </summary>
public class CreationResult<T>
{
    /// <summary>
    /// Created resource
    /// </summary>
    public T Resource { get; set; }

    /// <summary>
    /// Type of resource creation
    /// </summary>
    public CreationType CreationType { get; set; }

    /// <summary>
    /// Long running job id for resource creation
    /// </summary>
    public string JobId { get; set; }
}
