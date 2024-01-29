#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class DHControlNode : DHControlBase
{
    [JsonProperty("type")]
    public override DHControlType Type
    {
        get => DHControlType.Node;
        set { }
    }

    [JsonProperty("groupId")]
    public required Guid GroupId { get; set; }

    [JsonProperty("domains")]
    public required IEnumerable<string> Domains { get; set; }

    [JsonProperty("schedule")]
    public DHControlSchedule? Schedule { get; set; }

    [JsonProperty("assessmentId")]
    public Guid? AssessmentId { get; set; }
}
