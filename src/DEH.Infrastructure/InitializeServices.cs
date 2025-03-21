namespace DEH.Infrastructure;

using Application.Abstractions.Catalog;
using Catalog;
using Configurations;
using DEH.Domain.LogAnalytics;
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

    private static IBackfillCatalogRepository CreateBackfillCatalogRepository(
        IServiceProvider sp,
        string repositoryType,
        Database database)
    {
        var logger = sp.GetRequiredService<IDataEstateHealthRequestLogger>();
        return repositoryType switch
        {
            nameof(OkrBackfillCatalogRepository) => new OkrBackfillCatalogRepository(database, logger),
            nameof(KeyresultBackfillCatalogRepository) => new KeyresultBackfillCatalogRepository(database, logger),
            nameof(CdeBackfillCatalogRepository) => new CdeBackfillCatalogRepository(database, logger),
            _ => throw new ArgumentException($"Unknown repository type: {repositoryType}", nameof(repositoryType))
        };
    }

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
        string[] repositoryTypes =
        [
            nameof(OkrBackfillCatalogRepository),
            nameof(KeyresultBackfillCatalogRepository),
            nameof(CdeBackfillCatalogRepository)
        ];

        foreach (string repositoryType in repositoryTypes)
        {
            services.AddKeyedScoped<IBackfillCatalogRepository>(
                repositoryType,
                (sp, key) =>
                {
                    var database = sp.GetRequiredKeyedService<Database>(BackfillCatalogDatabaseKey);
                    return CreateBackfillCatalogRepository(sp, repositoryType, database);
                });
        }
    }

    public static void AddJobRunLogsDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddSingleton<IDEHAnalyticsJobLogsRepository, DEHAnalyticsJobLogsRepository>();
       
    }
}