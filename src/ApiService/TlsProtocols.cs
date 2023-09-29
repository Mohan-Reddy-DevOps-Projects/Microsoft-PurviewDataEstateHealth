// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Net.Security;

/// <summary>
/// Defines the <see cref="TlsProtocols" />.
/// </summary>
internal class TlsProtocols
{
    /// <summary>
    /// Defines the CipherSuitesPolicy.
    /// </summary>
    public static readonly CipherSuitesPolicy CipherSuitesPolicy = new CipherSuitesPolicy(
        new TlsCipherSuite[]
        {
            // TLS 1.3 cipher suites:
            TlsCipherSuite.TLS_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_AES_256_GCM_SHA384,

            // TLS 1.2 cipher suites:
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA384,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA384,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA256
        });
}
