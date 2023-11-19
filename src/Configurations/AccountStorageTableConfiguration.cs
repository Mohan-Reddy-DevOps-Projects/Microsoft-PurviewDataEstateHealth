// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configuration for processing storage account table
/// </summary>
public class AccountStorageTableConfiguration : StorageTableConfiguration
{
}

/// <summary>
/// Configuration for Data Governance Account storage table
/// </summary>
public abstract class StorageTableConfiguration : AuthConfiguration
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
