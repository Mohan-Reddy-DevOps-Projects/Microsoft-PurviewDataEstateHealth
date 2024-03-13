// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces;

/// <summary>
/// The scoped service for request header handling.
/// </summary>
public interface IRequestHeaderContext : IRequestContext
{
    /// <summary>
    /// Http Context
    /// </summary>
    HttpContext HttpContent { get; }

    /// <summary>
    /// Forwarded URL from gateway
    /// </summary>
    string ForwardedUrl { get; }

    /// <summary>
    /// Azure Resource Id
    /// TODO revisit the need for this
    /// </summary>
    string ResourceId { get; }

    //Account + Customer claims

    /// <summary>
    /// Microsoft Purview Account User Actions
    /// </summary>
    string AccountUserActions { get; }

    // Authorization claims

    /// <summary>
    /// Client audience of the call
    /// </summary>
    string ClientAudience { get; }


    /// <summary>
    /// Client Principal name of the call
    /// </summary>
    string ClientPrincipalName { get; }

    /// <summary>
    /// Client scope of the call
    /// </summary>
    string ClientScope { get; }

    /// <summary>
    /// ClientAppIdAcr of the call
    /// </summary>
    string ClientAppIdAcr { get; }

    /// <summary>
    /// ClientAppId of the call
    /// </summary>
    string ClientAppId { get; }

    /// <summary>
    /// Subject Name of the client
    /// </summary>
    string ClientSubject { get; }

    /// <summary>
    /// Issuer of the client
    /// </summary>
    string ClientIssuer { get; }

    /// <summary>
    /// Groups of the client
    /// </summary>
    string ClientGroups { get; }

    /// <summary>
    /// Claim names of the client
    /// </summary>
    string ClientClaimNames { get; }

    /// <summary>
    /// ResourceId of the client
    /// </summary>
    string ClientResourceId { get; }

    /// <summary>
    /// Given name of the client
    /// </summary>
    string ClientGivenName { get; }

    /// <summary>
    /// Family name of the client
    /// </summary>
    string ClientFamilyName { get; }

    /// <summary>
    /// The header for the alternate security identifier found in the JWT.
    /// </summary>
    string ClientAltSecId { get; }

    /// <summary>
    /// client PUID.
    /// </summary>
    string ClientPuid { get; }

    /// <summary>
    /// Authorization obligation type for caller.
    /// </summary>
    string AuthorizationObligationType { get; }

    /// <summary>
    /// Authorization obligations for caller. This should be removed once Obligations is available
    /// </summary>
    string AuthorizationObligations { get; }

    /// <summary>
    /// Authorization obligations for caller.
    /// </summary>
    public ObligationDictionary Obligations { get; }

    /// <summary>
    /// Sku of the Purview account. (Classic/Free/Enterprise)
    /// </summary>
    public PurviewAccountSku PurviewAccountSku { get; set; }
}
