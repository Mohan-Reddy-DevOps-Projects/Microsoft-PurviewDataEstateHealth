// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using AspNetCore.Authentication;
using Common;
using Configurations;
using DGP.ServiceBasics.Errors;
using Loggers;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;

internal class CertificateAuthenticationHandler : AuthenticationHandler<CertificateAuthenticationOptions>
{
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly EnvironmentConfiguration environmentConfig;
    private readonly IOptions<AllowListedCertificateConfiguration> whitelistedCertsconfig;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CertificateAuthenticationHandler" /> class.
    /// </summary>
    public CertificateAuthenticationHandler(
        IOptionsMonitor<CertificateAuthenticationOptions> options,
        IOptions<EnvironmentConfiguration> environmentConfig,
        IOptions<AllowListedCertificateConfiguration> whitelistedCertsconfig,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        IDataEstateHealthRequestLogger logger)
        : base(options, loggerFactory, encoder)
    {
        this.environmentConfig = environmentConfig.Value;
        this.logger = logger;
        this.whitelistedCertsconfig = whitelistedCertsconfig;
    }

    /// <summary>
    ///     Handles the authenticate asynchronous.
    /// </summary>
    /// <returns></returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            this.logger.LogTrace("Verifying claims and certificates");
            var clientCertificate = this.Context.Connection.ClientCertificate;
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
    ///     Validates the client certificate.
    /// </summary>
    /// <exception cref="ServiceError">
    ///     2 - Missing client certificate in request.
    ///     or
    ///     3 - Failed to perform chain validation {clientCertificate.Thumbprint}
    ///     or
    ///     4 - Certificate is not within valid time range.
    ///     or
    ///     5 - Invalid certificate in request : {clientCertificate.Thumbprint}
    /// </exception>
    /// <param name="clientCertificate"></param>
    protected void ValidateClientCertificate(X509Certificate2 clientCertificate)
    {
        this.logger.LogTrace($"Verifying client certificate subject [{clientCertificate?.SubjectName?.Name}], issuer [{clientCertificate?.IssuerName?.Name}]");

        switch (clientCertificate)
        {
            case null when this.environmentConfig.IsDevelopmentEnvironment():
                // Don't bother.
                return;
            case null:
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

        // 2) Build and validate the chain (trust + revocation)
        var chain = new X509Chain();
        chain.ChainPolicy = new X509ChainPolicy
        {
            RevocationMode = X509RevocationMode.Online,
            RevocationFlag = X509RevocationFlag.EntireChain,
            VerificationFlags = X509VerificationFlags.NoFlag,

            // 3) Enforce EKU for Client Authentication
            ApplicationPolicy = { new Oid("1.3.6.1.5.5.7.3.2") }
        };

        if (!chain.Build(clientCertificate))
        {
            string errors = String.Join(", ",
                chain.ChainStatus.Select(s => s.StatusInformation.Trim()));
            throw new ServiceError(
                ErrorCategory.AuthenticationError,
                ErrorCode.InvalidClientCertificate.Code,
                $"Certificate chain validation failed: {errors}"
            ).ToException();
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

        var validCertIssuers = this.whitelistedCertsconfig.Value.AllowListedIssuerNames;

        string simpleIssuerName = clientCertificate.GetNameInfo(X509NameType.SimpleName, true);

        if (!validCertIssuers.Any(issuer => simpleIssuerName.Contains(issuer, StringComparison.OrdinalIgnoreCase)))
        {
            this.logger.LogWarning("CertRejected: issuer {Issuer} does not contain whitelisted issuer", clientCertificate.Issuer);
            throw new ServiceError(
                    ErrorCategory.AuthenticationError,
                    ErrorCode.InvalidClientCertificate.Code,
                    $"Certificate issuer does not contain whitelisted issuer : {clientCertificate.Thumbprint} {clientCertificate.Subject} {clientCertificate.Issuer}.")
                .ToException();
        }

        var allowedSubjectNames = this.whitelistedCertsconfig
            .Value.GetAllAllowListedSubjectNames();

        string simpleSubjectName = clientCertificate.GetNameInfo(X509NameType.SimpleName, false);
        if (!allowedSubjectNames.Contains(simpleSubjectName, StringComparer.OrdinalIgnoreCase))
        {
            this.logger.LogWarning("CertRejected: subject {Subject} does not contain whitelisted subject", simpleSubjectName);
            throw new ServiceError(
                    ErrorCategory.AuthenticationError,
                    ErrorCode.InvalidClientCertificate.Code,
                    $"Certificate subject does not contain whitelisted subject : {clientCertificate.Thumbprint} {clientCertificate.Subject} {clientCertificate.Issuer}.")
                .ToException();
        }
    }
}