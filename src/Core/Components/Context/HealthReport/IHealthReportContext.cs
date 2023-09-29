// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;

/// <summary>
/// Health report context
/// </summary>
public interface IHealthReportContext : IHealthReportListContext
{
    /// <summary>
    /// Health report id
    /// </summary>
    Guid ReportId { get; init; }
}
