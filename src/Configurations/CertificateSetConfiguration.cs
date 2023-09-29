// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Configurations;

using System;

/// <summary>
/// Configuration for a set of certificates
/// Certificates are a dictionary to allow referencing a certificate by its key name
/// </summary>
public class CertificateSetConfiguration : BaseCertificateConfiguration
{
    /// <summary>
    /// Common key vault uri
    /// </summary>
    public string CommonKeyVaultUri { get; set; }

    /// <summary>
    /// How often the certificates should be refreshed
    /// </summary>
    public TimeSpan RefreshRate { get; set; } = TimeSpan.Zero;
}
