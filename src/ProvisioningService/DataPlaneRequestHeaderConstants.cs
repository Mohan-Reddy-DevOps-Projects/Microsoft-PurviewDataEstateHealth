// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

/// <summary>
/// Gateway rest header constants
/// </summary>
internal static class DataPlaneRequestHeaderConstants
{
    /// <summary>
    /// The header client audience
    /// </summary>
    public readonly static string HeaderClientAudience = "x-ms-client-audience";

    /// <summary>
    /// The header client object id
    /// </summary>
    public readonly static string HeaderClientObjectId = "x-ms-client-object-id";

    /// <summary>
    /// The header client principal name
    /// </summary>
    public readonly static string HeaderClientPrincipalName = "x-ms-client-principal-name";

    /// <summary>
    /// The header client scope
    /// </summary>
    public readonly static string HeaderClientScope = "x-ms-client-scope";

    /// <summary>
    /// The header client application id acr
    /// </summary>
    public readonly static string HeaderClientAppIdAcr = "x-ms-client-app-id-acr";

    /// <summary>
    /// The header client application id
    /// </summary>
    public readonly static string HeaderClientAppId = "x-ms-client-app-id";

    /// <summary>
    /// The header client tenant id
    /// </summary>
    public readonly static string HeaderClientTenantId = "x-ms-client-tenant-id";

    /// <summary>
    /// The header client subject
    /// </summary>
    public readonly static string HeaderClientSubject = "x-ms-client-subject";

    /// <summary>
    /// The header client issuer
    /// </summary>
    public readonly static string HeaderClientIssuer = "x-ms-client-issuer";

    /// <summary>
    /// The header client groups
    /// </summary>
    public readonly static string HeaderClientGroups = "x-ms-client-groups";

    /// <summary>
    /// The header client claim names
    /// </summary>
    public readonly static string HeaderClientClaimNames = "x-ms-client-claim-names";

    /// <summary>
    /// The header account name
    /// </summary>
    public readonly static string HeaderAccountName = "x-ms-account-name";

    /// <summary>
    /// The header account id
    /// </summary>
    public readonly static string HeaderAccountId = "x-ms-account-id";

    /// <summary>
    /// The header account resource type.
    /// </summary>
    public readonly static string HeaderAccountResourceType = "x-ms-account-resource-type";

    /// <summary>
    /// The header catalog id
    /// </summary>
    public readonly static string HeaderCatalogId = "x-ms-catalog-id";

    /// <summary>
    /// The header account resource id encoded
    /// </summary>
    public readonly static string HeaderAccountResourceIdEncoded = "x-ms-account-resource-id-encoded";

    /// <summary>
    /// The header account user roles
    /// </summary>
    public readonly static string HeaderAccountUserRoles = "x-ms-account-user-roles";

    /// <summary>
    /// The header correlation request id
    /// </summary>
    public readonly static string HeaderCorrelationRequestId = "x-ms-correlation-request-id";

    /// <summary>
    /// The header forwarded url
    /// </summary>
    public readonly static string HeaderForwardedUrl = "x-forwarded-url";

    /// <summary>
    /// The header user agent
    /// </summary>
    public readonly static string HeaderUserAgent = "User-Agent";

    /// <summary>
    /// User token with scheme header
    /// </summary>
    public readonly static string HeaderUserTokenWithScheme = "x-ms-user-token";

    /// <summary>
    /// Accept language header
    /// </summary>
    public readonly static string HeaderAcceptLanguage = "Accept-Language";
}
