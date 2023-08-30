// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Logger;

/// <summary>
/// Available Purview Share log tables
/// </summary>
public enum PurviewShareLogTable
{
    /// <summary>
    /// Client cert auth event table
    /// </summary>
    ClientCertificateAuthLogEvent,

    /// <summary>
    /// Artifact store event table
    /// </summary>
    ArtifactStoreServiceLogEvent,
    /// <summary>
    /// Metadata service event table
    /// </summary>
    MetadataServiceLogEvent,

    /// <summary>
    /// Job definition event table
    /// </summary>
    JobDefinitionLogEvent,

    /// <summary>
    /// Job history event table
    /// </summary>
    JobHistoryLogEvent,

    /// <summary>
    /// Job storage event table
    /// </summary>
    JobStorageLogEvent,

    /// <summary>
    /// Job event table
    /// </summary>
    JobLogEvent,

    /// <summary>
    /// General Purview share event table
    /// </summary>
    PurviewShareLogEvent,

    /// <summary>
    /// Telemetry event table
    /// </summary>
    TelemetryLogEvent
}
