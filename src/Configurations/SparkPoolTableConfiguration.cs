// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configuration for Data Governance Spark pool table
/// </summary>
public class SparkPoolTableConfiguration : AuthConfiguration
{
    /// <summary>
    /// Table service uri
    /// </summary>
    public string TableServiceUri { get; set; }

    /// <summary>
    /// Table name
    /// </summary>
    public string TableName { get; set; }
}
