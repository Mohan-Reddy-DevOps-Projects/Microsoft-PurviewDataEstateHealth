// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Net;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// A utility class for converting forwarded certificate headers.
/// </summary>
public static class CertificateHeaderConverter
{
    /// <summary>
    /// Converts a forwarded certificate header to a X509Certificate2.
    /// </summary>
    /// <param name="headerValue">The value of the forwarded certificate header.</param>
    public static X509Certificate2 Convert(string headerValue)
    {
        // The format of the forwarded client certificate header is a
        // semicolon separated list of key value pairs, e.g.,
        //
        // Hash=....;Cert="...";Chain="...";
        //
        // We need to extract the Cert key. It contains the client certificate
        // in URL encoded PEM format.

        if (string.IsNullOrEmpty(headerValue))
        {
            return null;
        }

        const string certKeyNameAndSeparator = "Cert=";

        string certKeyValuePair = headerValue.Split(';').FirstOrDefault(key => key.StartsWith(certKeyNameAndSeparator));
        if (string.IsNullOrEmpty(certKeyValuePair))
        {
            return null;
        }

        string certValue = certKeyValuePair.Substring(certKeyNameAndSeparator.Length).Trim('"');

        return X509Certificate2.CreateFromPem(WebUtility.UrlDecode(certValue));
        
    }
}
