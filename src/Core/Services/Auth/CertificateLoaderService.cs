// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using global::Azure.Security.KeyVault.Certificates;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Common;
using Microsoft.Purview.DataGovernance.Reporting.Common;
using ErrorCode = Common.ErrorCode;

/// <summary>
/// Loads a certificate from certificate store, file or key vault
/// </summary>
public class CertificateLoaderService : ICertificateLoaderService
{
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly TokenCredential credentials;
    private Dictionary<string, X509Certificate2> certCache = new();
    private Dictionary<string, X509Certificate2> oldCertCache = new();
    private readonly IOptions<CertificateSetConfiguration> certConfig;
    private readonly CancellationTokenSource refreshCancellationTokenSource = new();
    private readonly object cleanupLock = new();

    private sealed record NamedCertificate(string CertificateName, X509Certificate2 Certificate);

    /// <summary>
    /// Public constructor
    /// </summary>
    public CertificateLoaderService(
        IKeyVaultAccessorService keyVaultAccessorService,
        IDataEstateHealthRequestLogger logger,
        IOptions<CertificateSetConfiguration> certConfig,
        AzureCredentialFactory credentialFactory)
    {
        this.keyVaultAccessorService = keyVaultAccessorService;
        this.logger = logger;
        this.credentials = credentialFactory.CreateDefaultAzureCredential();
        this.certConfig = certConfig;
    }

    /// <summary>
    /// Initializes the cache.
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        var certificateClient = new CertificateClient(new Uri(this.certConfig.Value.CommonKeyVaultUri), this.credentials);


        Dictionary<string, X509Certificate2> filledCertCache = new();
        SecretClient certSecretClient = new(new Uri(this.certConfig.Value.CommonKeyVaultUri), this.credentials);
        CertificateClient certCertClient = new(new Uri(this.certConfig.Value.CommonKeyVaultUri), this.credentials);
        var certificateDetails = certificateClient.GetPropertiesOfCertificatesAsync();
        List<Task<NamedCertificate>> certTasks = new();

        await foreach (CertificateProperties certificateDetail in certificateDetails)
        {
            if (certificateDetail.Enabled.Value)
            {
                certTasks.Add(this.LoadFromKeyVaultAsync(certificateDetail.Name, CancellationToken.None));
            }
        }

        NamedCertificate[] namedCertificates = await Task.WhenAll(certTasks);

        this.certCache = namedCertificates.ToDictionary(
            namedCert => namedCert.CertificateName,
            namedCert => namedCert.Certificate);

        //We don't need to track this task because we use a cancellation token to stop it when the class is being disposed or destroyed.
        _ = Task.Run(async () =>
        {
            TimeSpan refreshRate = this.certConfig.Value.RefreshRate == TimeSpan.Zero
                ? TimeSpan.FromMinutes(10) : this.certConfig.Value.RefreshRate;

            while (!this.refreshCancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(refreshRate, this.refreshCancellationTokenSource.Token);

                //Wait to clean up old certs for the next iteration in case they are being used in requests
                lock (this.cleanupLock)
                {
                    Cleanup(this.oldCertCache);
                    this.oldCertCache = new Dictionary<string, X509Certificate2>();
                }

                if (this.refreshCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                var newCertCache = new ConcurrentDictionary<string, X509Certificate2>();
                try
                {
                    var refreshTasks = this.certCache.Select(async cert =>
                    {
                        var newCert = await this.LoadFromKeyVaultAsync(cert.Key, CancellationToken.None);
                        newCertCache.TryAdd(cert.Key, newCert.Certificate);
                    });

                    await Task.WhenAll(refreshTasks);
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Error occurred when refreshing certs", ex);
                }

                if (this.refreshCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                this.oldCertCache = this.certCache;
                this.certCache = newCertCache.ToDictionary(i => i.Key, i => i.Value);
            }
        });
    }

    /// <summary>
    /// Load certificate
    /// </summary>
    /// <returns>Certificate loaded from the configured store</returns>
    public async Task<X509Certificate2> LoadAsync(string secretName, CancellationToken cancellationToken)
    {
        // construct the cache key
        var uriBuilder = new UriBuilder(this.certConfig.Value.CommonKeyVaultUri)
        {
            Path = secretName
        };

        string cacheKey = uriBuilder.ToString();

        // return value if present in cache
        // load the certificate from akv otherwise

        if (!this.certCache.TryGetValue(secretName, out X509Certificate2 loadedCertificate))
        {
            loadedCertificate = (await this.LoadFromKeyVaultAsync(secretName, cancellationToken)).Certificate;
        }

        // validate the certificate
        if (loadedCertificate == null)
        {
            throw new ServiceError(
                    ErrorCategory.DownStreamError,
                    ErrorCode.CertificateLoader_NotFound.Code,
                    $"Certificate could not be loaded from {cacheKey} or was invalid according to the validation procedure")
                .ToException();
        }

        if (!loadedCertificate.HasPrivateKey)
        {
            throw new ServiceError(
                    ErrorCategory.DownStreamError,
                    ErrorCode.CertificateLoader_MissingPrivateKey.Code,
                    $"Certificate with subject {loadedCertificate.Subject} loaded from {this.certConfig.Value.CommonKeyVaultUri} has no private key")
                .ToException();
        }

        // ECR Drill rotation validation
        this.logger.LogTrace(
            FormattableString.Invariant(
                $"Loaded certificate with SN: {loadedCertificate.SubjectName.Name} and thumbprint: {loadedCertificate.Thumbprint} from KeyVault"));

        return loadedCertificate;
    }

    private async Task<NamedCertificate> LoadFromKeyVaultAsync(string secretName, CancellationToken cancellationToken)
    {
        KeyVaultSecret secret = await this.keyVaultAccessorService.GetSecretAsync(secretName, cancellationToken);
        X509Certificate2 certificate = new(Convert.FromBase64String(secret.Value), (string)null);

        X509Chain chain = new()
        {
            //Can't set the revocation mode to "NoCheck" due to MS cert chain policy
            ChainPolicy =
            {
                VerificationFlags = X509VerificationFlags.AllFlags,
                RevocationFlag = X509RevocationFlag.ExcludeRoot
            }
        };

        // Validate certificate
        if (!chain.Build(certificate))
        {
            this.logger.LogWarning("Unable to verify certificate.");

            chain.Dispose();
            certificate.Dispose();
            certificate = null;
        }

        return new NamedCertificate(secretName, certificate);
    }

    /// <summary>
    /// Binds a cert to a message handler
    /// </summary>
    /// <param name="httpMessageHandler"></param>
    /// <param name="certName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task BindAsync(HttpMessageHandler httpMessageHandler, string certName, CancellationToken cancellationToken)
    {
        if (certName == null)
        {
            throw new ArgumentNullException(nameof(certName));
        }

        X509Certificate2 certificate = await this.LoadAsync(certName, cancellationToken);

        if (certificate != null)
        {
            if (httpMessageHandler is HttpClientHandler httpClientHandler)
            {
                httpClientHandler.ClientCertificates.Add(certificate);
            }
            else if (httpMessageHandler is SocketsHttpHandler socketsHttpHandler)
            {
                socketsHttpHandler.SslOptions.ClientCertificates ??= new X509CertificateCollection();
                socketsHttpHandler.SslOptions.ClientCertificates.Add(certificate);
                socketsHttpHandler.SslOptions.LocalCertificateSelectionCallback = (object sender, string targetHost, X509CertificateCollection localCertificates,
                    X509Certificate remoteCertificate, string[] acceptableIssuers) => certificate;
            }
        }
        else
        {
            throw new InvalidOperationException($"Client certificate with name {certName} not found");
        }
    }

    /// <summary>
    /// Certificates must be disposed to get rid of key and cer files that get created on disk
    /// when certificates are loaded from raw byte arrays
    /// </summary>
    private static void Cleanup(Dictionary<string, X509Certificate2> certificates)
    {
        foreach (KeyValuePair<string, X509Certificate2> certificate in certificates)
        {
            certificates[certificate.Key].Dispose();
        }
    }

    /// <summary>
    /// Disposes the class
    /// </summary>
    public void Dispose()
    {
        this.refreshCancellationTokenSource.Cancel();

        lock (this.cleanupLock)
        {
            Cleanup(this.certCache);
            Cleanup(this.oldCertCache);

            this.certCache = new Dictionary<string, X509Certificate2>();
            this.oldCertCache = new Dictionary<string, X509Certificate2>();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Cancels the long-running task when the class is destroyed
    /// </summary>
    ~CertificateLoaderService()
    {
        if (!this.refreshCancellationTokenSource.IsCancellationRequested)
        {
            this.refreshCancellationTokenSource.Cancel();
        }
    }
}
