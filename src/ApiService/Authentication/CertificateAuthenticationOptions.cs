// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Certificate authentication options
/// </summary>
public class CertificateAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Certificate authentication scheme
    /// </summary>
    public const string CertificateAuthenticationScheme = "Certificate";

    /// <summary>
    /// Gets or sets the challenge to put in the "WWW-Authenticate" header.
    /// </summary>
    public string Challenge { get; set; } = CertificateAuthenticationScheme;

}
