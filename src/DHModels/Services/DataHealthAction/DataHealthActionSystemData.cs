// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

#nullable disable
namespace Microsoft.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;
using System;

public class DataHealthActionSystemData
{
    [JsonProperty("createAt")]
    public String CreateAt { get; set; }

    [JsonProperty("lastModifiedAt")]
    public String LastModifiedAt { get; set; }

    [JsonProperty("lastHintAt")]
    public String LastHintAt { get; set; }

    [JsonProperty("lastModifiedBy")]
    public Guid LastModifiedBy { get; set; }

    [JsonProperty("hintCount")]
    public int HintCount { get; set; }
}


