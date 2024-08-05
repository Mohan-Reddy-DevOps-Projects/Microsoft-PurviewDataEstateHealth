// <copyright file="FabricOnelakeClientFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.StorageConfig
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.HttpClient;
    using System;
    using System.Net.Http;

    public class FabricOnelakeClientFactory : ClientFactory<FabricOnelakeClient>, IDisposable
    {
        private readonly IDataEstateHealthRequestLogger logger;

        public static string HttpClientName = "FabricOnelakeClient";

        protected override string ClientName => HttpClientName;

        public FabricOnelakeClientFactory(
            IHttpClientFactory httpClientFactory,
            IDataEstateHealthRequestLogger logger) : base(httpClientFactory, logger)
        {
            this.logger = logger;
        }

        protected override FabricOnelakeClient ConfigureClient(HttpClient httpClient)
        {
            return new FabricOnelakeClient(
                httpClient,
                this.logger);
        }
    }
}
