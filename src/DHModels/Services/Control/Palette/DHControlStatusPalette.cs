#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;

using Microsoft.Purview.DataEstateHealth.DHModels.Attributes;
using Newtonsoft.Json;
using System;

[CosmosDBContainer("DHControlStatusPalette")]
public class DHControlStatusPalette
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }

    [JsonProperty("color")]
    public required uint Color { get; set; }
}
