// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// Outgoing endpoints for Data Estate Health
/// </summary>
public enum EndPointType
{
    /// <summary>
    /// Unknown endpoint, default.
    /// </summary>
    Unknown,

    /// <summary>
    /// Artifact store
    /// </summary>
    ArtifactStore,

    /// <summary>
    /// The graph endpoint.
    /// </summary>
    Graph,

    /// <summary>
    /// The azure key vault endpoint.
    /// </summary>
    KeyVault,

    /// <summary>
    /// Metadata service endpoint
    /// </summary>
    MetadataStore,

    /// <summary>
    /// RBac endpoint
    /// </summary>
    Rbac
}
