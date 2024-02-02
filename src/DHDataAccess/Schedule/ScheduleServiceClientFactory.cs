// <copyright file="ScheduleServiceClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient;
    using System;
    using System.Net.Http;

    public class ScheduleServiceClientFactory : ClientFactory<ScheduleServiceClient>, IDisposable
    {
        private readonly ScheduleConfiguration config;
        private readonly IDataEstateHealthRequestLogger logger;

        public static string HttpClientName = "ScheduleServiceClient";

        protected override string ClientName => HttpClientName;

        public ScheduleServiceClientFactory(
            IOptions<ScheduleConfiguration> config,
            IHttpClientFactory httpClientFactory,
            IDataEstateHealthRequestLogger logger) : base(httpClientFactory, logger)
        {
            this.config = config.Value;
            this.logger = logger;
        }

        protected override ScheduleServiceClient ConfigureClient(HttpClient httpClient)
        {
            return new ScheduleServiceClient(
                httpClient,
                new Uri(this.config.Endpoint),
                this.logger);
        }
    }
}
