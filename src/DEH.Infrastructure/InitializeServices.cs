namespace DEH.Infrastructure;

using Application.Abstractions.Catalog;
using Catalog;
using Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class InitializeServices
{
    public static void AddCatalogDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions()
            .Configure<CatalogAdapterConfiguration>(configuration.GetSection(CatalogAdapterConfiguration.ConfigSectionName));

        HttpClientSettings httpClientSettings = new()
        {
            Name = CatalogHttpClientFactory.HttpClientName,
            UserAgent = "DGHealth",
            RetryCount = 3
        };

        services.AddCustomHttpClient<CatalogAdapterConfiguration>(httpClientSettings);

        services.AddSingleton<ICatalogHttpClientFactory, CatalogHttpClientFactory>();
    }
}