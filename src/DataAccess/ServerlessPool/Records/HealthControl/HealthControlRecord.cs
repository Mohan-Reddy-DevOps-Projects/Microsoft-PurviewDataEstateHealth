// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for Health Control.
/// </summary>
public class HealthControlRecord : BaseRecord
{
    /// <summary>
    /// Actual Value
    /// </summary>
    [DataColumn("ActualValue")]
    public double ActualValue { get; set; }

    /// <summary>
    /// Last refresh date
    /// </summary>
    [DataColumn("LastRefreshedAt ")]
    public DateTime LastRefreshedAt { get; set; }
}
