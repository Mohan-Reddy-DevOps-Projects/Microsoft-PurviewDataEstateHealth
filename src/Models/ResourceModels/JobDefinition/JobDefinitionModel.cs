// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System.Text.Json;

/// <summary>
/// Spark pool model.
/// </summary>
public class JobDefinitionModel
{
    /// <summary>
    /// Gets or sets the job partition.
    /// </summary>
    public string JobPartition { get; set; }

    /// <summary>
    /// Gets or sets the job id.
    /// </summary>
    public string JobId { get; set; }

    /// <summary>
    /// Gets or sets the callback.
    /// </summary>
    public string Callback { get; set; }

    /// <summary>
    /// Gets or sets last job status.
    /// </summary>
    public string LastExecutionStatus { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
