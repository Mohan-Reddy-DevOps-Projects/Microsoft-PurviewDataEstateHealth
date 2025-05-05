// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using AspNetCore.Authentication;
using AspNetCore.Authentication.Certificate;
using AspNetCore.Authorization;
using Configurations;
using DGP.ServiceBasics.Errors;
using IdentityModel.S2S.Extensions.AspNetCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

/// <summary>
///     Extension methods to add different authentication types
/// </summary>
internal static class ClientSecurityControlsExtensions
{
    private static AuthenticationBuilder AddCertificateAuthentication(
        this AuthenticationBuilder builder)
    {
        return builder.AddScheme<CertificateAuthenticationOptions, CertificateAuthenticationHandler>(
            CertificateAuthenticationDefaults.AuthenticationScheme,
            CertificateAuthenticationDefaults.AuthenticationScheme,
            options => new CertificateAuthenticationOptions());
    }

    internal static IServiceCollection AddApplicationSecurityControls(this WebApplicationBuilder builder)
    {
        // Get the certificate configuration
        using var tempServiceProvider = builder.Services.BuildServiceProvider();
        var certConfigOptions = tempServiceProvider.GetService<IOptions<AllowListedCertificateConfiguration>>();
        var certConfig = certConfigOptions?.Value;
        if (certConfig == null)
        {
            throw new ServiceException("Certificate configuration is not set");
        }

        string appId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")!;
        // Add AzureAd configuration
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
            ["AzureAd:TenantId"] = "common",
            ["AzureAd:ClientId"] = appId ?? "73c2949e-da2d-457a-9607-fcc665198967",
            ["AzureAd:RequireAuthorizationHeader"] = "false",
            ["AzureAd:UseProtocolHandlers"] = "true",
            ["AzureAd:InboundPolicies:0:Label"] = "data-plane",
            ["AzureAd:InboundPolicies:0:AuthenticationSchemes:0"] = "ClientCertificate",
            ["AzureAd:InboundPolicies:0:ClientId"] = appId ?? "c2b3a6f0-88cc-4cf5-931e-0bb5e06ebfda",
            ["AzureAd:InboundPolicies:0:CertificatePolicy:Cloud"] = "Public",
            ["AzureAd:InboundPolicies:0:CertificatePolicy:CertificateUsage"] = "ClientAuth"
        });

        // Add the PermittedDomainNames from the certificate configuration
        var allAllowListedSubjectNames = certConfig.GetAllAllowListedSubjectNames();
        for (int i = 0; i < allAllowListedSubjectNames.Count; i++)
        {
            builder.Configuration[$"AzureAd:InboundPolicies:0:CertificatePolicy:PermittedDomainNames:{i}"] = allAllowListedSubjectNames[i];
        }

        builder.Services
            .AddAuthentication(S2SAuthenticationDefaults.AuthenticationScheme)
            .AddCertificateAuthentication()
            .AddMiseWithDefaultModules(builder.Configuration);

        builder.Services.AddSingleton<IAuthorizationHandler, CertificateAuthorizationHandler>();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("ClientCertOnly", policy =>
                    policy
                        .AddAuthenticationSchemes(CertificateAuthenticationDefaults.AuthenticationScheme)
                        .AddRequirements(new CertificateAuthorizationHandlerRequirement()));
        }
        else
        {
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("ClientCertOnly", policy =>
                    policy
                        .AddAuthenticationSchemes(CertificateAuthenticationDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .AddRequirements(new CertificateAuthorizationHandlerRequirement())
                        .RequireClaim(ClaimTypes.Dns, certConfig.GetAllAllowListedSubjectNames()));
        }

        return builder.Services;
    }
}