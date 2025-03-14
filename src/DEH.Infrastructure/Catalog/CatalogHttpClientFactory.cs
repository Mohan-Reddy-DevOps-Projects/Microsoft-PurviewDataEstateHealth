namespace DEH.Infrastructure.Catalog;

using Application.Abstractions.Catalog;
using Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;

public class CatalogHttpClientFactory : ClientFactory<CatalogHttpClient>, ICatalogHttpClientFactory
{
    protected override string ClientName { get; } = HttpClientName;

    public static string HttpClientName => "CatalogHttpClient";

    private readonly CatalogAdapterConfiguration _config;
    private readonly IDataEstateHealthRequestLogger _logger;

    public CatalogHttpClientFactory(IHttpClientFactory httpClientFactory, IDataEstateHealthRequestLogger logger, IOptions<CatalogAdapterConfiguration> config)
        : base(httpClientFactory, logger)
    {
        this._config = config.Value;
        this._logger = logger;
    }

    protected override CatalogHttpClient ConfigureClient(HttpClient httpClient)
    {
        httpClient.Timeout = TimeSpan.FromSeconds(60 * 10);
        return new CatalogHttpClient(httpClient, new Uri(this._config.Endpoint), this._logger);
    }

    public new ICatalogHttpClient GetClient()
    {
        return base.GetClient();
    }
}