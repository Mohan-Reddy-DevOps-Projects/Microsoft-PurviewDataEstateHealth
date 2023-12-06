// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json;

/// <summary>
/// Refresh details model
/// </summary>
public sealed class RefreshDetailsModel
{
    /// <summary>
    /// Dataset Id.
    /// </summary>
    public Guid DatasetId { get; set; }

    /// <summary>
    /// Profile Id.
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    /// Workspace Id.
    /// </summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// Start time.
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// End time.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Status.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Current refresh type.
    /// </summary>
    public string CurrentRefreshType { get; set; }

    /// <summary>
    /// Number of attempts.
    /// </summary>
    public int? NumberOfAttempts { get; set; }

    /// <summary>
    /// Messages.
    /// </summary>
    public IList<EngineMessage> Messages { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

/// <summary>
/// Engine message model
/// </summary>
public sealed class EngineMessage
{
    /// <summary>
    /// Code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Type.
    /// </summary>
    public string Type { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
