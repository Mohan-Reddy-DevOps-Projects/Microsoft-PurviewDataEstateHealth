// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Processing storage properties model.
/// </summary>
public class ProcessingStoragePropertiesModel
{
    /// <summary>
    /// Gets or sets the created at time.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the DNS zone.
    /// </summary>
    public string DnsZone { get; set; }

    /// <summary>
    /// Gets or sets the endpoint suffix.
    /// </summary>
    public string EndpointSuffix { get; set; }

    /// <summary>
    /// Gets or sets the last modified at time.
    /// </summary>
    public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the location.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string ResourceId { get; set; }

    /// <summary>
    /// Gets or sets the SKU.
    /// </summary>
    public string Sku { get; set; }
}
