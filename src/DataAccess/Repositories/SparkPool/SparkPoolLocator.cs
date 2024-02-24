// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Holds information needed to retrieve spark pool by Id
/// </summary>
public class SparkPoolLocator
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SparkPoolLocator"/> class.
    /// </summary>
    public SparkPoolLocator(string id, string name)
    {
        this.PartitionId = id;
        this.Name = name;
    }

    /// <summary>
    /// The row key.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The partition key. This is set as the account id.
    /// </summary>
    public string PartitionId { get; }
}
