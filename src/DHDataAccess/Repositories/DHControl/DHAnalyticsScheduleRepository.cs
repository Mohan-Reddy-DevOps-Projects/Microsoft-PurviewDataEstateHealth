namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHAnalyticsScheduleRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
    CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DHControlScheduleStoragePayloadWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    private const string ContainerName = "DHStorageSchedule";

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private string DatabaseName => configuration["cosmosDb:settingsDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHAnalytics is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    private readonly IDataEstateHealthRequestLogger logger = logger;

    public async Task<IEnumerable<DHControlScheduleStoragePayloadWrapper>> QueryAnalyticsScheduleAsync(DHControlScheduleType scheduleType)
    {
        var methodName = nameof(QueryAnalyticsScheduleAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, {this.AccountIdentifier.Log}"))
        {
            try
            {
                var query = this.CosmosContainer.GetItemLinqQueryable<DHControlScheduleStoragePayloadWrapper>(
                    requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
                    .Where(c => c.Type == scheduleType && c.AccountId == this.AccountIdentifier.AccountId)
                    .ToFeedIterator();

                var results = new List<DHControlScheduleStoragePayloadWrapper>();
                while (query.HasMoreResults)
                {
                    var response = await this.retryPolicy.ExecuteAsync(
                        (context) => query.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{scheduleType}_{this.AccountIdentifier.ConcatenatedId}")
                    ).ConfigureAwait(false);
                    this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, {this.AccountIdentifier.Log}", ex);
                throw;
            }
        }
    }
}