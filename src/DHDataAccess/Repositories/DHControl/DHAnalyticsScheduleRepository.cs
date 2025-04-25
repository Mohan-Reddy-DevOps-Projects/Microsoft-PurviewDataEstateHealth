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

    /// <summary>
    /// Query analytics schedule for the current account
    /// </summary>
    /// <param name="scheduleType">The schedule type to query</param>
    /// <returns>Collection of schedule payload wrappers</returns>
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

    /// <summary>
    /// Query analytics schedule for a specific account and tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="accountId">The account ID</param>
    /// <param name="scheduleType">The schedule type to query</param>
    /// <returns>Collection of schedule payload wrappers</returns>
    public async Task<IEnumerable<DHControlScheduleStoragePayloadWrapper>> QueryAnalyticsScheduleAsync(string tenantId, string accountId, DHControlScheduleType scheduleType)
    {
        var methodName = nameof(QueryAnalyticsScheduleAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, TenantId: {tenantId}, AccountId: {accountId}"))
        {
            try
            {
                var partitionKey = new PartitionKey(tenantId);
                var query = this.CosmosContainer.GetItemLinqQueryable<DHControlScheduleStoragePayloadWrapper>(
                    requestOptions: new QueryRequestOptions { PartitionKey = partitionKey })
                    .Where(c => c.Type == scheduleType && c.AccountId == accountId)
                    .ToFeedIterator();

                var results = new List<DHControlScheduleStoragePayloadWrapper>();
                while (query.HasMoreResults)
                {
                    var response = await this.retryPolicy.ExecuteAsync(
                        (context) => query.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{scheduleType}_{tenantId}_{accountId}")
                    ).ConfigureAwait(false);
                    this.cosmosMetricsTracker.LogCosmosMetrics(new AccountIdentifier { AccountId = accountId, TenantId = tenantId }, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, TenantId: {tenantId}, AccountId: {accountId}", ex);
                throw;
            }
        }
    }
}