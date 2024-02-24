// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// DataAccessEventHubConfiguration
/// </summary>
public class DataAccessEventHubConfiguration : EventHubConfiguration
{
    private string eventHubName;
    private string containerName;

    /// <summary>
    /// Data access event hub name.
    /// </summary>
    public string DataAccessEventHubName { get => this.eventHubName; set => this.EventHubName = this.eventHubName = value; }

    /// <summary>
    /// Data access event hub processed events checkpoints.
    /// </summary>
    public string DataAccessCheckpointContainerName { get => this.containerName; set => this.EventCheckpointsContainerName = this.containerName = value; }
}
