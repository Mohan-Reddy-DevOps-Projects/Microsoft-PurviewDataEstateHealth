// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

/// <summary>
/// A Health score class.
/// </summary>
[KnownType(typeof(DataGovernanceScore))]
[KnownType(typeof(DataCurationScore))]
[KnownType(typeof(DataQualityScore))]
[JsonConverter(typeof(HealthScoreConverter))]
public abstract partial class HealthScore
{
    /// <summary>
    /// Kind of health score.
    /// </summary>
    [Required]
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("kind")]
    public HealthScoreKind ScoreKind { get; set; }

    /// <summary>
    /// Score name.
    /// </summary>
    [ReadOnly(true)]
    public string Name { get; internal set; }

    /// <summary>
    /// Score description.
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; internal set; }

    /// <summary>
    /// Report Id.
    /// </summary>
    [ReadOnly(true)]
    public Guid ReportId { get; internal set; }
}
