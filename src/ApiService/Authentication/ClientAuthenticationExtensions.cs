// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Extension methods to add different authentication types
/// </summary>
internal static class ClientAuthenticationExtensions
{
    internal static AuthenticationBuilder AddCertificateAuthentication(
        this AuthenticationBuilder builder)
    {
        return builder.AddScheme<CertificateAuthenticationOptions, CertificateAuthenticationHandler>(
            CertificateAuthenticationOptions.CertificateAuthenticationScheme,
            CertificateAuthenticationOptions.CertificateAuthenticationScheme,
            options => new CertificateAuthenticationOptions());
    }
}

