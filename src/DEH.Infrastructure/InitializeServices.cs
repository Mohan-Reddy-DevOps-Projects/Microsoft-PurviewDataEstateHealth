namespace DEH.Infrastructure;

using Application.Abstractions.Catalog;
using Catalog;
using Configurations;
using Domain.Backfill.Catalog;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Repositories;

public static class InitializeServices
{
    public const string BackfillCatalogDatabaseKey = "BackfillCatalogDatabase";

    public static void AddCatalogDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        const string backfillCatalogDatabaseName = "dgh-Backfill";
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

        services.AddKeyedScoped<Database>(
            BackfillCatalogDatabaseKey,
            (sp, key) =>
            {
                var cosmosClient = sp.GetRequiredService<IDefaultCosmosClient>().Client;
                return cosmosClient.GetDatabase(backfillCatalogDatabaseName);
            });

        services.AddKeyedScoped<IBackfillCatalogRepository>(
            nameof(OkrBackfillCatalogRepository),
            (sp, key) =>
            {
                var database = sp.GetRequiredKeyedService<Database>(BackfillCatalogDatabaseKey);
                var logger = sp.GetRequiredService<IDataEstateHealthRequestLogger>();
                return new OkrBackfillCatalogRepository(database, logger);
            });

        services.AddKeyedScoped<IBackfillCatalogRepository>(
            nameof(KeyresultBackfillCatalogRepository),
            (sp, key) =>
            {
                var database = sp.GetRequiredKeyedService<Database>(BackfillCatalogDatabaseKey);
                var logger = sp.GetRequiredService<IDataEstateHealthRequestLogger>();
                return new KeyresultBackfillCatalogRepository(database, logger);
            });

        services.AddKeyedScoped<IBackfillCatalogRepository>(
            nameof(CdeBackfillCatalogRepository),
            (sp, key) =>
            {
                var database = sp.GetRequiredKeyedService<Database>(BackfillCatalogDatabaseKey);
                var logger = sp.GetRequiredService<IDataEstateHealthRequestLogger>();
                return new CdeBackfillCatalogRepository(database, logger);
            });
    }
}