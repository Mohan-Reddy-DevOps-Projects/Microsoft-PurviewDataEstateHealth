// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a capacity model with details about specific resource capacities.
/// </summary>
public class CapacityModel
{
    /// <summary>
    /// Gets the unique identifier for the capacity.
    /// </summary>
    [JsonPropertyName("CapacityId")]
    public Guid CapacityId { get; }

    /// <summary>
    /// Gets the region associated with the capacity.
    /// </summary>
    [JsonPropertyName("Region")]
    public string Region { get; }

    /// <summary>
    /// Gets the SKU name for the capacity.
    /// </summary>
    [JsonPropertyName("SkuName")]
    public string SkuName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CapacityModel"/> class.
    /// </summary>
    /// <param name="capacityId">The unique identifier for the capacity.</param>
    /// <param name="region">The region associated with the capacity.</param>
    /// <param name="skuName">The SKU name for the capacity.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the provided arguments are null or if the capacityId is an empty Guid.
    /// </exception>
    public CapacityModel(Guid capacityId, string region, string skuName)
    {
        this.CapacityId = capacityId != Guid.Empty ? capacityId : throw new ArgumentNullException(nameof(capacityId));
        this.Region = region ?? throw new ArgumentNullException(nameof(region));
        this.SkuName = skuName ?? throw new ArgumentNullException(nameof(skuName));
    }

    /// <summary>
    /// Returns a string representation of the <see cref="CapacityModel"/>.
    /// </summary>
    /// <returns>A string representing the capacity details.</returns>
    public override string ToString()
    {
        return $"CapacityId: {this.CapacityId}, SkuName: {this.SkuName}, Region: {this.Region}";
    }
}
