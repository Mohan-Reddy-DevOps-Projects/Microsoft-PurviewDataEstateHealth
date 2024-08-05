// <copyright file="PurviewMITokenClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient;
    using System;
    using System.Net.Http;

    public class PurviewMITokenClientFactory : ClientFactory<PurviewMITokenClient>, IDisposable
    {
        private readonly DHDataQualityJobManagerConfiguration config;
        private readonly IDataEstateHealthRequestLogger logger;

        public static string HttpClientName = "PurviewMITokenClient";

        protected override string ClientName => HttpClientName;

        public PurviewMITokenClientFactory(
            IOptions<DHDataQualityJobManagerConfiguration> config,
            IHttpClientFactory httpClientFactory,
            IDataEstateHealthRequestLogger logger) : base(httpClientFactory, logger)
        {
            this.config = config.Value;
            this.logger = logger;
        }

        protected override PurviewMITokenClient ConfigureClient(HttpClient httpClient)
        {
            return new PurviewMITokenClient(
                httpClient,
                new Uri(this.config.Endpoint),
                this.logger);
        }
    }
}
