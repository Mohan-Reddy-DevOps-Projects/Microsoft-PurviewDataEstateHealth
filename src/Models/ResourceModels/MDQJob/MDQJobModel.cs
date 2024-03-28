// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json;

/// <summary>
/// Spark pool model.
/// </summary>
public class MDQJobModel
{
    /// <summary>
    /// Gets or sets the account identifier.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Gets or sets the tenant identifier.
    /// </summary>
    public Guid DQJobId { get; set; }

    /// <summary>
    /// Gets or sets job status.
    /// </summary>
    public string JobStatus { get; set; }

    /// <summary>
    /// Gets or sets created time.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets record time.
    /// </summary>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
