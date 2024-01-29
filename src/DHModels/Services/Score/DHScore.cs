#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

public class DHScore
{
    [JsonProperty("id")]
    public required Guid Id { get; set; }

    [JsonProperty("time")]
    public required long Time { get; set; }

    [JsonProperty("controlId")]
    public required Guid ControlId { get; set; }

    [JsonProperty("computingJobId")]
    public required Guid ComputingJobId { get; set; }

    [JsonProperty("entitySnapshot")]
    public required JObject EntitySnapshot { get; set; }

    [JsonProperty("score")]
    public IDictionary<string, byte> Score { get; set; } = new Dictionary<string, byte>();

    [JsonProperty("aggregatedScore")]
    public double AggregatedScore { get; set; }
}
