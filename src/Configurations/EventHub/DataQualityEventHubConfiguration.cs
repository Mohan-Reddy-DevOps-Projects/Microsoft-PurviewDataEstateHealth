// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// DataQualityEventHubConfiguration
/// </summary>
public class DataQualityEventHubConfiguration : EventHubConfiguration
{
    private string eventHubName;
    private string containerName;

    /// <summary>
    /// Data quality event hub name.
    /// </summary>
    public string DataQualityEventHubName { get => this.eventHubName; set => this.EventHubName = this.eventHubName = value; }

    /// <summary>
    /// Data quality event hub processed events checkpoints.
    /// </summary>
    public string DataQualityCheckpointContainerName { get => this.containerName; set => this.EventCheckpointsContainerName = this.containerName = value; }
}
