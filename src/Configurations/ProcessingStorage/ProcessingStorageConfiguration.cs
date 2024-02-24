// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Processing storage configuration.
/// </summary>
public class ProcessingStorageConfiguration
{
    /// <summary>
    /// Gets or sets the storage account name prefix.
    /// </summary>
    public string StorageNamePrefix { get; set; }

    /// <summary>
    /// Gets or sets the azure region.
    /// </summary>
    public string AzureRegion { get; set; }

    /// <summary>
    /// Gets or sets the resource group name.
    /// </summary>
    public string ResourceGroupName { get; set; }

    /// <summary>
    /// Gets or sets the subscriptionId.
    /// </summary>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the tenantId.
    /// </summary>
    public Guid TenantId { get; set; }
}
