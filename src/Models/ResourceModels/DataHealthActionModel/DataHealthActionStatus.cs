// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

[JsonConverter(typeof(StringEnumConverter))]
public enum DataHealthActionStatus
{
    Active,

    Resolved
}
