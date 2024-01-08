// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using Microsoft.Purview.DataGovernance.Common;

/// <summary>
/// EventHubConfiguration
/// </summary>
public abstract class EventHubConfiguration: AuthConfiguration
{
    /// <summary>
    /// ConsumerGroup
    /// </summary>
    public string ConsumerGroup { get; set; }

    /// <summary>
    /// EvenHubNamespace
    /// </summary>
    public string EventHubNamespace { get; set; }

    /// <summary>
    /// EventHubName
    /// </summary>
    public string EventHubName { get; set; }

    /// <summary>
    /// EventCheckpointsContainerName
    /// </summary>
    public string EventCheckpointsContainerName { get; set; }

    /// <summary>
    /// MaxEventsToProcess
    /// </summary>
    public int MaxEventsToProcess { get; set; }
}
