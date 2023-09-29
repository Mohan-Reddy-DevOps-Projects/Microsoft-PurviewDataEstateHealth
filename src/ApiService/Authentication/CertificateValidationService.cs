// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.Identity.ServerAuthorization;
using LogLevel = Identity.ServerAuthorization.LogLevel;
using System.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// Validate client certificates
/// </summary>
public class CertificateValidationService : IDisposable
{
    private readonly DynamicCertificateValidatorFactory dynamicCertificateValidatorFactory;

    private readonly Dictionary<string, IEnumerable<ICertificateValidator>> certificateValidatorsGroup;

    private bool disposedValue;

    /// <summary>
    /// Public constructor
    /// </summary>
    /// <param name="config">Authorization configuration</param>
    /// <param name="logger">Logger</param>
    public CertificateValidationService(IOptions<ClientCertificateConfiguration> config, IDataEstateHealthLogger logger)
    {
        this.dynamicCertificateValidatorFactory = new DynamicCertificateValidatorFactory(
            (level, message) =>
            {
                switch (level)
                {
                    case LogLevel.Error:
                        logger.LogError(message, null);

                        break;
                    case LogLevel.Informational:
                        logger.LogInformation(message);

                        break;
                    case LogLevel.Warning:
                        logger.LogWarning(message);

                        break;
                }
            });

        this.certificateValidatorsGroup = new Dictionary<string, IEnumerable<ICertificateValidator>>();

        foreach (CertificateSet set in Enum.GetValues(typeof(CertificateSet)))
        {
            IEnumerable<string> allowListedSubjectNames = config.Value.GetAllowListedSubjectNames(set);
            IEnumerable<string> allowListedIssuerNames = config.Value.AllowListedIssuerNames;

            // Multiple issuers can be configured, create one validator per issuer
            var validators = new List<ICertificateValidator>();
            foreach (string issuerName in allowListedIssuerNames)
            {
                string[] subjectNames = allowListedSubjectNames.ToArray();

                ICertificateValidator validator = this.dynamicCertificateValidatorFactory.Create(
                    environment: Identity.ServerAuthorization.Environment.Public,
                    usage: CertificateUsage.ClientAuth,
                    caName: issuerName,
                    allowDenyPolicy: new AllowDenyPolicy()
                    {
                        AllowedSubjectNames = subjectNames,
                    });

                validators.Add(validator);
            }

            this.certificateValidatorsGroup.Add(set.ToString(), validators);
        }
    }

    /// <summary>
    /// Validate a client certificate
    /// </summary>
    /// <param name="set">certificate set</param>
    /// <param name="clientCertificate">Client certificate to validate</param>
    /// <returns>True if certificate is valid, false otherwise</returns>
    public bool ValidateCertificate(string set, X509Certificate2 clientCertificate)
    {
        if (this.certificateValidatorsGroup.TryGetValue(
            set,
            out IEnumerable<ICertificateValidator> certificateValidators))
        {
            foreach (ICertificateValidator certificateValidator in certificateValidators)
            {
                AuthorizationResult authorizationResult = certificateValidator.IsAuthorized(clientCertificate);
                if (authorizationResult.Authorized)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Effective dispose
    /// </summary>
    /// <param name="disposing">If true, called from explicit dispose</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.dynamicCertificateValidatorFactory?.Dispose();
            }

            this.disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
