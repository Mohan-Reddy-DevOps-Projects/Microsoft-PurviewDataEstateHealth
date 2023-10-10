// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations for the Serverless Pool.
/// </summary>
public sealed class ServerlessPoolConfiguration
{
    /// <summary>
    /// Gets or sets the development endpoint for this Synapse database.
    /// </summary>
    public string DevelopmentEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the SQL endpoint for database communication.
    /// </summary>
    public string SqlEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the name of the database to be used in the serverless pool.
    /// </summary>
    public string Database { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether statistics are enabled for the serverless pool.
    /// When true, the serverless pool collects and provides statistical information about its operations.
    /// </summary>
    public bool StatisticsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of connections allowed in the connection pool for the specific connection string.
    /// </summary>
    public int MinPoolSize { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of connections allowed in the connection pool for the specific connection string.
    /// </summary>
    public int MaxPoolSize { get; set; }

    /// <summary>
    /// Synapse storage account name.
    /// </summary>
    public string StorageAccount { get; set; }
}
