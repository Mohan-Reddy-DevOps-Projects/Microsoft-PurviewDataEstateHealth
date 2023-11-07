// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Base query
/// </summary>
public class BaseQuery
{
    /// <summary>
    /// Database
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Container path.
    /// </summary>
    public string ContainerPath { get; set; }

    /// <summary>
    /// Filter clause.
    /// </summary>
    public string FilterClause { get; set; }
}
