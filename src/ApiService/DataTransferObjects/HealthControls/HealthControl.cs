// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

/// <summary>
/// A Health control class.
/// </summary>
[KnownType(typeof(DataGovernanceHealthControl))]
[KnownType(typeof(DataQualityHealthControl))]
[JsonConverter(typeof(HealthControlConverter))]
public abstract partial class HealthControl
{
    /// <summary>
    /// Control Id.
    /// </summary>
    [ReadOnly(true)]
    public Guid Id { get; internal set; }

    /// <summary>
    /// Control kind.
    /// </summary>
    [ReadOnly(true)]
    public HealthControlKind Kind { get; internal set; }
}
