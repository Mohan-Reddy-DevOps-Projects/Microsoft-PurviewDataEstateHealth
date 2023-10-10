// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

/// <summary>
/// A health action data transfer object.
/// </summary>
public class HealthAction
{
    /// <summary>
    /// Health action Id
    /// </summary>
    [ReadOnly(true)]
    public Guid Id { get; internal set; }

    /// <summary>
    /// Health action name
    /// </summary>
    [ReadOnly(true)]
    public string Name { get; internal set; }

    /// <summary>
    /// Health action description
    /// </summary>
    [ReadOnly(true)]
    public string Description { get; internal set; }

    /// <summary>
    /// Health action owner detail
    /// </summary>
    [ReadOnly(true)]
    public OwnerContact OwnerContact { get; internal set; }

    /// <summary>
    /// Health control name
    /// </summary>
    [ReadOnly(true)]
    public string HealthControlName { get; internal set; }

    /// <summary>
    /// Health control state
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public HealthControlState HealthControlState { get; set; }

    /// <summary>
    /// Health action created at 
    /// </summary>
    [ReadOnly(true)]
    public DateTime CreatedAt { get; internal set; }

    /// <summary>
    /// Health action last refreshed at 
    /// </summary>
    [ReadOnly(true)]
    public DateTime LastRefreshedAt { get; internal set; }

    /// <summary>
    /// Target details
    /// </summary>
    [ReadOnly(true)]
    public IEnumerable<TargetDetails> TargetDetailsList { get; internal set; }
}
