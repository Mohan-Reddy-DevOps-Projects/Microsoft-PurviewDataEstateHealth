// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.DHModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Clients;
using System;
using System.Net.Http;

public class DataQualityServiceClientFactory : ClientFactory<DataQualityHttpClient>
{
    private readonly DataQualityServiceConfiguration config;

    public static string HttpClientName { get; } = "DataQualityServiceClient";

    protected override string ClientName => HttpClientName;

    private readonly IDataEstateHealthRequestLogger logger;

    /// <summary>
    /// Public constructor
    /// </summary>
    /// <param name="config">Data quality client configuration</param>
    /// <param name="httpClientFactory">Http client factory</param>
    /// <param name="logger">Logger</param>
    public DataQualityServiceClientFactory(
        IOptions<DataQualityServiceConfiguration> config,
        IHttpClientFactory httpClientFactory,
        IDataEstateHealthRequestLogger logger) : base(httpClientFactory, logger)
    {
        this.config = config.Value;
        this.logger = logger;
    }

    protected override DataQualityHttpClient ConfigureClient(HttpClient httpClient)
    {
        httpClient.Timeout = TimeSpan.FromSeconds(60 * 10);
        return new DataQualityHttpClient(httpClient, new Uri(this.config.Endpoint), this.logger);
    }
}
