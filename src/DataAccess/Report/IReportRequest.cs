// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// The report request.
/// </summary>
public interface IReportRequest
{
    /// <summary>
    /// The profile id.
    /// </summary>
    Guid ProfileId { get; }

    /// <summary>
    /// The workspace id.
    /// </summary>
    Guid WorkspaceId { get; }

    /// <summary>
    /// The report id.
    /// </summary>
    Guid ReportId { get; }
}
