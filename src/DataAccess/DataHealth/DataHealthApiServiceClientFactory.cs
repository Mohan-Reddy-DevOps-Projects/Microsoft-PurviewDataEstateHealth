// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess
{
    using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Options;
    using System;
    using System.Net.Http;

    /// <inheritdoc/>
    internal sealed class DataHealthApiServiceClientFactory : ClientFactory<DataHealthApiServiceClient>, IDisposable
    {
        private readonly DataHealthApiServiceConfiguration config;
        private readonly IDataEstateHealthRequestLogger logger;

        public static string HttpClientName { get; } = "DataHealthApiServiceClient";

        protected override string ClientName => HttpClientName;

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="config">Metadata client configuration</param>
        /// <param name="httpClientFactory">Http client factory</param>
        /// <param name="logger">Logger</param>
        public DataHealthApiServiceClientFactory(
            IOptions<DataHealthApiServiceConfiguration> config,
            IHttpClientFactory httpClientFactory,
            IDataEstateHealthRequestLogger logger) : base(httpClientFactory, logger)
        {
            this.config = config.Value;
            this.logger = logger;
        }

        protected override DataHealthApiServiceClient ConfigureClient(HttpClient httpClient)
        {
            return new DataHealthApiServiceClient(httpClient, new Uri(this.config.Endpoint), this.logger);
        }
    }
}