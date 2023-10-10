// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Newtonsoft.Json;

/// <summary>
/// Target details class.
/// </summary>
public class TargetDetails
{
    /// <summary>
    ///Target Kind
    /// </summary>
    [JsonProperty("targetKind")]
    public TargetKind TargetKind { get; set; }

    /// <summary>
    ///Target Id
    /// </summary>
    [JsonProperty("targetId")]
    public Guid TargetId { get; set; }

    /// <summary>
    ///Target Name
    /// </summary>
    [JsonProperty("targetName")]
    public string TargetName { get; set; }

    /// <summary>
    ///Owner Contact
    /// </summary>
    [JsonProperty("ownerContact")]
    public OwnerContact OwnerContact { get; set; }
}
