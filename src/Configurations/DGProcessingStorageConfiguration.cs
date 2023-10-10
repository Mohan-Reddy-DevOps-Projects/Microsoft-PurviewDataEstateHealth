// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations related to processing storage account
/// </summary>
public class DGProcessingStorageConfiguration
{
    /// <summary>
    /// Processing storage account name.
    /// </summary>
    public string StorageAccountName { get; set; }

    /// <summary>
    /// Processing storage account subscription id.
    /// </summary>
    public string SubscriptionId { get; set; }

    /// <summary>
    /// Processing storage account resource group.
    /// </summary>
    public string ResourceGroup { get; set; }

    /// <summary>
    /// Processing storage account region.
    /// </summary>
    public string Region { get; set; }
}
