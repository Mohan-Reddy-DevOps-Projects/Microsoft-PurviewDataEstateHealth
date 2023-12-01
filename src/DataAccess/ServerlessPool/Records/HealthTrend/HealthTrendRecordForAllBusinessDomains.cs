// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using System;

internal class HealthTrendRecordForAllBusinessDomains : BaseRecord
{
    /// <summary>
    /// Trend last refreshed at.
    /// </summary>
    [DataColumn("LastRefreshedAt")]
    public DateTime LastRefreshedAt { get; set; }

    /// <summary>
    /// A generic holder for whatever column is being selected.
    /// </summary>
    public int HealthTrendDataValue { get; set; }
}
