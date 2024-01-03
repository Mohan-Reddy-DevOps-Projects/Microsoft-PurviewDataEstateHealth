// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

internal class CertificateAuthenticationHandler : AuthenticationHandler<CertificateAuthenticationOptions>
{
    private static readonly Dictionary<string, CertificateSet> controllerToCertificateSetMap;

    private readonly IDataEstateHealthRequestLogger logger;
    private readonly CertificateValidationService certificateValidationService;
    private readonly EnvironmentConfiguration environmentConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateAuthenticationHandler" /> class.
    /// </summary>
    public CertificateAuthenticationHandler(
        IOptionsMonitor<CertificateAuthenticationOptions> options,
        IOptions<EnvironmentConfiguration> environmentConfig,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IDataEstateHealthRequestLogger logger,
        CertificateValidationService certificateValidationService)
        : base(options, loggerFactory, encoder)
    {
        this.environmentConfig = environmentConfig.Value;
        this.logger = logger;
        this.certificateValidationService = certificateValidationService;
    }

    static CertificateAuthenticationHandler()
    {
        string controllerPostfix = "Controller";
        controllerToCertificateSetMap = typeof(DataPlaneController).Assembly
            .GetTypes()
            .Select(t => (TypeName: t.Name, Attr: t.GetCustomAttribute<CertificateConfigAttribute>()))
            .Where(t => t.Attr != null)
            .Distinct()
            .ToDictionary(
                t => t.TypeName.EndsWith(controllerPostfix)
                    ? t.TypeName.Substring(0, t.TypeName.Length - controllerPostfix.Length)
                    : t.TypeName,
                t => t.Attr.CertificateSet);
    }

    /// <summary>
    /// Handles the authenticate asynchronous.
    /// </summary>
    /// <returns></returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            this.logger.LogTrace("Verifying claims and certificates");
            X509Certificate2 clientCertificate = this.Context.Connection.ClientCertificate;
            this.ValidateClientCertificate(clientCertificate);
        }
        catch (Exception exception)
        {
            this.logger.LogError("Error in client cert authentication", exception);
            return Task.FromResult(AuthenticateResult.Fail(exception));
        }

        return Task.FromResult(
            AuthenticateResult.Success(
                new AuthenticationTicket(
                    new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), this.Options.Challenge)),
                    new AuthenticationProperties(),
                    this.Options.Challenge)));
    }

    /// <summary>
    /// Validates the client certificate.
    /// </summary>
    /// <exception cref="ServiceError">
    /// 2 - Missing client certificate in request.
    /// or
    /// 3 - Failed to perform chain validation {clientCertificate.Thumbprint}
    /// or
    /// 4 - Certificate is not within valid time range.
    /// or
    /// 5 - Invalid certificate in request : {clientCertificate.Thumbprint}
    /// </exception>
    /// <param name="clientCertificate"></param>
    protected void ValidateClientCertificate(X509Certificate2 clientCertificate)
    {
        this.logger.LogTrace($"Verifying client certificate subject [{clientCertificate?.SubjectName?.Name}], issuer [{clientCertificate?.IssuerName?.Name}]");

        if (clientCertificate == null && this.environmentConfig.IsDevelopmentEnvironment())
        {
            // Don't bother.
            return;
        }

        if (clientCertificate == null)
        {
            throw new ServiceError(
                    ErrorCategory.AuthenticationError,
                    ErrorCode.MissingClientCertificate.Code,
                    "Missing client certificate in request.")
                .ToException();
        }

        if (!clientCertificate.Verify())
        {
            throw new ServiceError(
                    ErrorCategory.AuthenticationError,
                    ErrorCode.InvalidClientCertificate.Code,
                    $"Failed to perform validation {clientCertificate.Thumbprint} {clientCertificate.Subject} {clientCertificate.Issuer}.")
                .ToException();
        }

        if (DateTimeOffset.Compare(DateTimeOffset.UtcNow, clientCertificate.NotBefore) < 0 ||
            DateTimeOffset.Compare(DateTimeOffset.UtcNow, clientCertificate.NotAfter) > 0)
        {
            throw new ServiceError(
                    ErrorCategory.AuthenticationError,
                    ErrorCode.InvalidClientCertificate.Code,
                    $"Certificate is not within valid time range : {clientCertificate.Thumbprint} {clientCertificate.Subject} {clientCertificate.Issuer}.")
                .ToException();
        }

        if (!this.IsValidTrustedClientCertificate(clientCertificate))
        {
            throw new ServiceError(
                    ErrorCategory.AuthenticationError,
                    ErrorCode.InvalidClientCertificate.Code,
                    $"Invalid certificate in request : {clientCertificate.Thumbprint} {clientCertificate.Subject}.")
                .ToException();
        }
    }

    /// <summary>
    /// Determines whether [is valid trusted client certificate] [the specified client certificate].
    /// </summary>
    /// <param name="clientCertificate">The client certificate.</param>
    /// <returns>
    /// <c>true</c> if [is valid trusted client certificate] [the specified client certificate]; otherwise, <c>false</c>.
    /// </returns>
    protected bool IsValidTrustedClientCertificate(X509Certificate2 clientCertificate)
    {
        if (!(this.Context.Request.RouteValues["controller"] is string controllerName) ||
            !controllerToCertificateSetMap.TryGetValue(controllerName, out CertificateSet certificateSet))
        {
            throw new ServiceError(
                    ErrorCategory.ServiceError,
                    ErrorCode.NotAuthorized.Code,
                    "Controller certificate misconfigured")
                .ToException();
        }

        return this.certificateValidationService.ValidateCertificate(certificateSet.ToString(), clientCertificate);
    }
}
