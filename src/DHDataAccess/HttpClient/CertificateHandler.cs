// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient
{
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataGovernance.Reporting.Common;
    using System;
    using System.Net.Http;
    using System.Net.Security;
    using System.Threading.Tasks;

    /// <summary>
    /// A handler to fetch a certificate.
    /// </summary>
    public class CertificateHandler<T> : DelegatingHandler, ICertificateHandler where T : BaseDHCertificateConfiguration
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
            await this.certificateLoaderService.BindAsync(messageHandler, this.certConfiguration.CertificateName, default).ConfigureAwait(false);

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

    /// <summary>
    /// Interface to create handler for babylon metadata client.
    /// </summary>
    internal interface ICertificateHandler
    {
        /// <summary>
        /// Method to create http handler for babylon metadata client.
        /// </summary>
        /// <returns></returns>
        Task<SocketsHttpHandler> CreateHandlerAsync();
    }
}