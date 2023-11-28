// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Configurations related to artifact store service
/// </summary>
public class ArtifactStoreServiceConfiguration : BaseCertificateConfiguration
{
    /// <summary>
    /// Endpoint.
    /// </summary>
    public string Endpoint { get; set; }
}
