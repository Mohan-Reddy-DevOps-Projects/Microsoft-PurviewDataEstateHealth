// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;

using System.ComponentModel;

/// <summary>
/// Trend value class
/// </summary>
public class TrendValue
{
    /// <summary>
    /// Health TrendValue capture date
    /// </summary>
    [ReadOnly(true)]
    public DateTime CaptureDate { get; internal set; }

    /// <summary>
    /// Health TrendValue value
    /// </summary>
    [ReadOnly(true)]
    public string Value { get; internal set; }
}
