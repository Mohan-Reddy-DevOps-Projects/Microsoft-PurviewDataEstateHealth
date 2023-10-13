// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Text.Json;

/// <summary>
/// Dataset request.
/// </summary>
public sealed class ReportRequest : IReportRequest
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ReportRequest() { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="other"></param>
    public ReportRequest(IReportRequest other)
    {
        this.ProfileId = other.ProfileId;
        this.WorkspaceId = other.WorkspaceId;
        this.ReportId = other.ReportId;
    }

    /// <inheritdoc/>
    public Guid ProfileId { get; init; }

    /// <inheritdoc/>
    public Guid WorkspaceId { get; init; }

    /// <inheritdoc/>
    public Guid ReportId { get; init; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
