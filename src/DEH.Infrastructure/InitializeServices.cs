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
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

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

    // Create a custom retry policy for HTTP requests
    private static IAsyncPolicy<HttpResponseMessage> CreateCustomRetryPolicy(IDataEstateHealthRequestLogger logger)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests ||
                         r.StatusCode == HttpStatusCode.RequestTimeout ||
                         r.StatusCode >= HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(
                5, // Number of retries
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                (outcome, timeSpan, retryCount, context) =>
                {
                    if (outcome.Exception != null)
                    {
                        logger?.LogWarning($"Attempt {retryCount} failed: {outcome.Exception.Message}. Retrying after {timeSpan.TotalSeconds} seconds.");
                    }
                    else
                    {
                        logger?.LogWarning($"Attempt {retryCount} failed with status code {outcome.Result.StatusCode}. Retrying after {timeSpan.TotalSeconds} seconds.");
                    }
                }
            );
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

        services.AddCustomHttpClient<CatalogAdapterConfiguration>(httpClientSettings, 
            (serviceProvider, request, defaultPolicy) => 
            {
                var logger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
                var retryPolicy = CreateCustomRetryPolicy(logger);
                
                // Apply this policy by wrapping the default policy
                defaultPolicy.WrapAsync(retryPolicy);
            });

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