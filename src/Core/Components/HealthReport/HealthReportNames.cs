// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal static class HealthReportNames
{
    public const string DataGovernance = "Data governance";


    public static readonly HashSet<string> System = new()
    {
        DataGovernance
    };
}

/// <summary>
/// Names of the owners of the reports.
/// </summary>
public static class OwnerNames
{
    /// <summary>
    /// The name of the owner of the health report.
    /// </summary>
    public const string Health = "health";
}
