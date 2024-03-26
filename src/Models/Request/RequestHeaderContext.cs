// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.ResourceStack.Common.Instrumentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;

/// <inheritdoc />
public class RequestHeaderContext : RequestContext, IRequestHeaderContext
{
    private const string ForwardedUrlHeader = "x-forwarded-url";

    private const string HeaderClientSubject = "x-ms-client-subject";

    private const string HeaderClientGroups = "x-ms-client-groups";

    private const string HeaderClaimNames = "x-ms-client-claim-names";

    private const string HeaderClientResourceId = "x-ms-client-resource-id";

    private const string HeaderClientPuid = "x-ms-client-puid";

    private const string AccountNameHeader = "x-ms-account-name";

    private const string AccountIdHeader = "x-ms-account-id";

    private const string AccountUserActionsHeader = "x-ms-account-user-actions";

    private const string AccountResourceIdHeader = "x-ms-account-resource-id";

    private const string CatalogIdHeader = "x-ms-catalog-id";

    private const string HeaderClientTenantId = "x-ms-client-tenant-id";

    private const string AccountSkuName = "x-ms-account-sku-name";

    private const string AccountReconciled = "x-ms-account-reconciled";

    /// <summary>
    /// Account authorization obligation type header
    /// </summary>
    public const string HeaderAccountObligationType = "x-ms-account-authorization-obligation-type";

    /// <summary>
    /// Account authorization obligations header
    /// </summary>
    public const string HeaderAccountObligations = "x-ms-account-authorization-obligations";

    /// <summary>
    /// Header that conveys the auth obligation type
    /// </summary>
    public const string AuthObligationsV2 = "x-ms-account-authorization-obligations-v2";

    /// <summary>
    /// Instantiate instance of RequestHeaderContext.
    /// </summary>
    public RequestHeaderContext(IHttpContextAccessor httpContextAccessor)
    {
        this.HttpContent = httpContextAccessor?.HttpContext;
        IHeaderDictionary headers = httpContextAccessor?.HttpContext?.Request?.Headers;

        this.CorrelationId = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderCorrelationRequestId) ?? Guid.NewGuid().ToString();
        this.ForwardedUrl = headers.GetFirstOrDefault(ForwardedUrlHeader);

        this.AccountName = headers.GetFirstOrDefault(AccountNameHeader);
        this.AccountObjectId = headers.GetFirstOrDefaultGuid(AccountIdHeader);
        this.AccountUserActions = headers.GetFirstOrDefault(AccountUserActionsHeader);
        this.AccountResourceId = headers.GetFirstOrDefault(AccountResourceIdHeader);
        this.CatalogId = headers.GetFirstOrDefault(CatalogIdHeader);

        this.ClientAudience = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientAudience);
        this.ClientScope = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientScope);
        this.ClientObjectId = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientObjectId);
        this.ClientPrincipalName = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientPrincipalName)
            ?.Split('#')
            .Last();
        this.ClientAppIdAcr = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientAppIdAcr);
        this.ClientAppId = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientAppId);
        this.TenantId = headers.GetFirstOrDefaultGuid(HeaderClientTenantId);
        this.ClientSubject = headers.GetFirstOrDefault(HeaderClientSubject);
        this.ClientIssuer = headers.GetFirstOrDefault(RequestCorrelationContext.HeaderClientIssuer);
        this.ClientGroups = headers.GetFirstOrDefault(HeaderClientGroups);
        this.ClientClaimNames = headers.GetFirstOrDefault(HeaderClaimNames);
        this.ClientResourceId = headers.GetFirstOrDefault(HeaderClientResourceId);
        this.ApiVersion =
            httpContextAccessor?.HttpContext?.Request?.GetFirstOrDefaultQuery(
                RequestCorrelationContext.ParameterApiVersion) ?? string.Empty;
        this.ClientGivenName = DecodeBase64String(
            httpContextAccessor?.HttpContext?.Request?.Headers.GetFirstOrDefault(
                RequestCorrelationContext.HeaderGivenNameEncoded),
            RequestCorrelationContext.HeaderGivenNameEncoded,
            string.Empty);
        this.ClientFamilyName = DecodeBase64String(
            httpContextAccessor?.HttpContext?.Request?.Headers.GetFirstOrDefault(
                RequestCorrelationContext.HeaderFamilyNameEncoded),
            RequestCorrelationContext.HeaderFamilyNameEncoded,
            string.Empty);
        this.ClientPuid =
            httpContextAccessor?.HttpContext?.Request?.Headers.GetFirstOrDefault(
                HeaderClientPuid);

        this.ClientIpAddress = httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        this.AuthorizationObligationType =
            headers.GetFirstOrDefault(HeaderAccountObligationType);
        this.AuthorizationObligations = headers.GetFirstOrDefault(HeaderAccountObligations);

        string obligationsStr = headers.GetFirstOrDefault(AuthObligationsV2);
        if (!string.IsNullOrEmpty(obligationsStr))
        {
            var jsonObject = JObject.Parse(obligationsStr);
            var firstKey = jsonObject.Properties().FirstOrDefault()?.Name;
            if (firstKey?.StartsWith(Obligation.ContainerTypePrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                this.Obligations = JsonConvert.DeserializeObject<ObligationDictionary>(obligationsStr);
            }
            else if (firstKey?.StartsWith(Obligation.PermissionPrefix, StringComparison.OrdinalIgnoreCase) == true)
            {
                var permissionObligations = JsonConvert.DeserializeObject<Dictionary<string, Obligation>>(obligationsStr);
                var containerType = permissionObligations.FirstOrDefault().Value?.ContainerType;
                if (!string.IsNullOrEmpty(containerType))
                {
                    this.Obligations = new ObligationDictionary(new Dictionary<string, Dictionary<string, Obligation>>()
                        {
                            { containerType, permissionObligations }
                        });
                }
            }
        }

        string accountSkuName = headers.GetFirstOrDefault(AccountSkuName) ?? "";
        string accountReconciled = headers.GetFirstOrDefault(AccountReconciled) ?? "";
        this.PurviewAccountSku = accountSkuName.EqualsOrdinalInsensitively("Free")
                            ? PurviewAccountSku.Free
                            : accountReconciled.EqualsOrdinalInsensitively("1")
                                ? PurviewAccountSku.Enterprise
                                : PurviewAccountSku.Classic;
    }

    /// <inheritdoc />
    public HttpContext HttpContent { get; init; }

    /// <inheritdoc />
    public string ForwardedUrl { get; set; }

    /// <inheritdoc />
    public string ResourceId { get; set; }

    /// <inheritdoc />
    public string AccountUserActions { get; set; }

    /// <inheritdoc />
    public string ClientAudience { get; set; }

    /// <inheritdoc />
    public string ClientPrincipalName { get; set; }

    /// <inheritdoc />
    public string ClientScope { get; set; }

    /// <inheritdoc />
    public string ClientAppIdAcr { get; set; }

    /// <inheritdoc />
    public string ClientAppId { get; set; }

    /// <inheritdoc />
    public string ClientSubject { get; set; }

    /// <inheritdoc />
    public string ClientIssuer { get; set; }

    /// <inheritdoc />
    public string ClientGroups { get; set; }

    /// <inheritdoc />
    public string ClientClaimNames { get; set; }

    /// <inheritdoc />
    public string ClientResourceId { get; set; }

    /// <inheritdoc />
    public string ClientGivenName { get; set; }

    /// <inheritdoc />
    public string ClientFamilyName { get; set; }

    /// <inheritdoc />
    public string ClientAltSecId { get; set; }

    /// <inheritdoc />
    public string ClientPuid { get; set; }

    /// <inheritdoc />
    public string AuthorizationObligationType { get; set; }

    /// <inheritdoc />
    /// Need to remove this one once Obligations is available
    public string AuthorizationObligations { get; set; }

    public ObligationDictionary Obligations { get; private set; }

    /// <inheritdoc />
    public PurviewAccountSku PurviewAccountSku { get; set; }

    /// <summary>
    /// Returns decoded string from the header
    /// </summary>
    /// <param name="encodedString">Base 64 encoded</param>
    /// <param name="defaultHeader">default header value</param>
    /// <param name="defaultFieldValue">default field value</param>
    /// <returns></returns>
    private static string DecodeBase64String(string encodedString, string defaultHeader, string defaultFieldValue)
    {
        if (string.IsNullOrEmpty(encodedString) || string.Compare(
            encodedString,
            defaultHeader,
            StringComparison.OrdinalIgnoreCase) == 0)
        {
            return defaultFieldValue;
        }
        byte[] data = Convert.FromBase64String(encodedString);

        return Encoding.UTF8.GetString(data);
    }
}
