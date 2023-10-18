// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// Available Purview Share log tables
/// </summary>
public enum DataEstateHealthLogTable
{
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
    /// General Data estate health event table
    /// </summary>
    DataEstateHealthLogEvent,

    /// <summary>
    /// Telemetry event table
    /// </summary>
    TelemetryLogEvent,

    /// <summary>
    /// Metadata service event table
    /// </summary>
    MetadataServiceLogEvent,
}
