// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels;

/// <summary>
/// Properties to create a spark job
/// </summary>
public sealed class SparkJobRequest
{
    /// <summary>
    /// Gets or sets the configuration
    /// </summary>
    public Dictionary<string, string> Configuration { get; init; }

    /// <summary>
    /// Gets or sets the number of executors
    /// </summary>
    public int ExecutorCount { get; init; }

    /// <summary>
    /// Gets or sets the file name
    /// </summary>
    public string File { get; init; }

    /// <summary>
    /// Gets or sets the name job name
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets or sets the arguments to pass to the run manager
    /// </summary>
    public List<string> RunManagerArgument { get; init; }
}
