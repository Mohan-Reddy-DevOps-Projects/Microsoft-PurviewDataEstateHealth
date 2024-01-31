#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class DHControlNode : DHControlBase
{
    [JsonProperty("type")]
    public override required DHControlType Type
    {
        get => DHControlType.Node;
        set { }
    }

    [JsonProperty("groupId", Required = Required.Always)]
    public required Guid GroupId { get; set; }

    [JsonProperty("domains")]
    public IList<string> Domains { get; set; } = new List<string>();

    [JsonProperty("schedule")]
    public DHControlSchedule? Schedule { get; set; }

    [JsonProperty("assessmentId")]
    public Guid? AssessmentId { get; set; }
}
