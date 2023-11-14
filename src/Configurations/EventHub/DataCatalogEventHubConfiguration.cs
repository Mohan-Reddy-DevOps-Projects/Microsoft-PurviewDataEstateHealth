// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// DataCatalogEventHubConfiguration
/// </summary>
public class DataCatalogEventHubConfiguration : EventHubConfiguration
{
    private string eventHubName;
    private string containerName;

    /// <summary>
    /// Data catalog event hub name.
    /// </summary>
    public string DataCatalogEventHubName { get => this.eventHubName; set => this.EventHubName = this.eventHubName = value; }

    /// <summary>
    /// Data catalog event hub processed events checkpoints.
    /// </summary>
    public string DataCatalogCheckpointContainerName { get => this.containerName; set => this.EventCheckpointsContainerName = this.containerName = value; }
}
