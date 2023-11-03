// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Represents a request model for creating or updating a storage account.
/// </summary>
public sealed class StorageAccountRequestModel
{
    /// <summary>
    /// Gets or sets the name of the storage account.
    /// </summary>
    /// <value>
    /// The storage account name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the geographic location where the storage account will be created.
    /// </summary>
    /// <value>
    /// The location as a string, e.g., "WestUS2".
    /// </value>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the SKU for the storage account which determines the type of account.
    /// </summary>
    /// <value>
    /// The SKU name as a string, e.g., "Standard_LRS" for Standard Locally Redundant Storage.
    /// </value>
    public string Sku { get; set; }

    /// <summary>
    /// Gets or sets the name of the Azure resource group in which the storage account will be provisioned.
    /// </summary>
    /// <value>
    /// The resource group name.
    /// </value>
    public string ResourceGroup { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the Azure subscription under which the storage account will be provisioned.
    /// </summary>
    /// <value>
    /// The subscription ID as a <see cref="System.Guid"/>.
    /// </value>
    public Guid SubscriptionId { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier for the tenant that owns the subscription.
    /// </summary>
    /// <value>
    /// The tenant ID as a <see cref="Guid"/>.
    /// </value>
    public Guid TenantId { get; set; }
}
