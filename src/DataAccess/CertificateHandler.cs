// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// A handler to fetch a certificate.
/// </summary>
public class CertificateHandler<T> : DelegatingHandler, ICertificateHandler where T : BaseCertificateConfiguration
{
    private readonly ICertificateLoaderService certificateLoaderService;

    private readonly T certConfiguration;

    private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// summary
    /// </summary>
    public CertificateHandler(
        ICertificateLoaderService certificateLoaderService,
        IOptions<T> certConfiguration)
    {
        this.certConfiguration = certConfiguration.Value;
        this.certificateLoaderService = certificateLoaderService;
        this.InnerHandler = this.CreateHandlerAsync().Result;
    }

    /// <inheritdoc/>
    public async Task<SocketsHttpHandler> CreateHandlerAsync()
    {
        // Use SocketsHttpHandler rather then HttpClientHandler to enable multiple http2 connections
        // HttpClientHandler actually delegates the work to an internal SocketsHttpHandler instance,
        // but the EnableMultipleHttp2Connections property is not exposed.
        // We set the property so we can open additional connections to the same server when the
        // maximum number of concurrent streams is reached on all existing connections.
        var messageHandler = new SocketsHttpHandler()
        {
            ConnectTimeout = DefaultConnectTimeout,
            EnableMultipleHttp2Connections = true,
        };
        await this.certificateLoaderService.BindAsync(messageHandler, this.certConfiguration.CertificateName, default);
        
        messageHandler.SslOptions.RemoteCertificateValidationCallback = (message, cert, chain, errors) =>
        {
            if ((errors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                // Reset only [Flags] bit for certificate chain error. There might be other errors
                errors &= ~SslPolicyErrors.RemoteCertificateChainErrors;
            }

            return errors == SslPolicyErrors.None;
        };

        return messageHandler;
    }
}
