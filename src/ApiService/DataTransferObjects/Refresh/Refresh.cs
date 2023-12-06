// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A refresh data transfer object.
/// </summary>
public class Refresh
{
    /// <summary>
    /// Refresh Id
    /// </summary>
    [ReadOnly(true)]
    public string Id { get; internal set; }

    /// <summary>
    /// Refresh status
    /// </summary>
    [ReadOnly(true)]
    public string Status { get; internal set; }

    /// <summary>
    /// Refresh start time
    /// </summary>
    public DateTime? StartTime { get; internal set; }

    /// <summary>
    /// Refresh end time
    /// </summary>
    public DateTime? EndTime { get; internal set; }
}
