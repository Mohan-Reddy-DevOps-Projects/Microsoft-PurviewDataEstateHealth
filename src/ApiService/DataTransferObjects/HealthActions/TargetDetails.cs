// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// Target details class.
/// </summary>
public class TargetDetails
{
    /// <summary>
    /// Target kind.
    /// </summary>
    [ReadOnly(true)]
    [JsonConverter(typeof(StringEnumConverter))]
    public TargetKind TargetKind { get; internal set; }

    /// <summary>
    /// Target Id.
    /// </summary>
    [ReadOnly(true)]
    public Guid TargetId { get; internal set; }

    /// <summary>
    /// Target name.
    /// </summary>
    [ReadOnly(true)]
    public string TargetName { get; internal set; }

    /// <summary>
    /// Target name.
    /// </summary>
    [ReadOnly(true)]
    public OwnerContact OwnerContact { get; internal set; }
}
