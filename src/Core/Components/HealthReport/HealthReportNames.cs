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
