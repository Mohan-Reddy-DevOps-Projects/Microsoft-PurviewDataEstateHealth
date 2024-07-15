namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DHScoreRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
    CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DHScoreBaseWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    private const string ContainerName = "DHScore";

    private readonly IDataEstateHealthRequestLogger logger = logger;

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Azure.Cosmos.Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<IEnumerable<DHScoreAggregatedByControl>> QueryScoreGroupByControl(IEnumerable<string> controlIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
    {
        var methodName = nameof(QueryScoreGroupByControl);
        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, {this.AccountIdentifier.Log}"))
        {
            // Construct the SQL query
            var sqlQuery = new StringBuilder(@$"
SELECT
    c.ControlGroupId,
    c.ControlId,
    c.ScheduleRunId,
    c.ComputingJobId,
    MAX(c.Time) AS Time,
    SUM(c.ScoreSum) AS ScoreSum,
    SUM(c.ScoreCount) AS ScoreCount
FROM c WHERE c.AccountId = '{this.AccountIdentifier.AccountId}' ");

            // Add filtering conditions based on provided parameters
            if (domainIds != null && domainIds.Any())
            {
                // Assuming domainIds is a validated and sanitized list of IDs
                var domainIdFilter = string.Join(",", domainIds.Select(id => $"'{id}'"));
                // TODO: When added more supported artifact types, modify the SQL here.
                sqlQuery.Append($"AND ARRAY_CONTAINS([{domainIdFilter}], c.DataProductDomainId) ");
            }

            // Assuming controlIds is a validated and sanitized list of IDs
            var controlIdFilter = string.Join(",", controlIds.Select(id => $"'{id}'"));
            sqlQuery.Append($"AND ARRAY_CONTAINS([{controlIdFilter}], c.ControlId) ");

            if (start != null)
            {
                sqlQuery.Append($"AND c.Time >= '{start.Value.ToString("o")}' ");
            }

            if (end != null)
            {
                sqlQuery.Append($"AND c.Time <= '{end.Value.ToString("o")}' ");
            }

            if (status != null)
            {
                sqlQuery.Append($"AND c.DataProductStatus = '{status}' ");
            }

            sqlQuery.Append("GROUP BY c.ControlGroupId, c.ControlId, c.ScheduleRunId, c.ComputingJobId ");

            var queryDefinition = new QueryDefinition(sqlQuery.ToString());

            // Execute the query
            var queryResultSetIterator = this.CosmosContainer.GetItemQueryIterator<DHScoreSQLQueryResponse1>(queryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });
            var intermediateResults = new List<DHScoreAggregatedByControl>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var response = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
                this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response, queryDefinition.QueryText);
                intermediateResults.AddRange(response.Resource.Where(x => x.ScoreCount > 0).Select(x => new DHScoreAggregatedByControl
                {
                    ControlGroupId = x.ControlGroupId,
                    ControlId = x.ControlId,
                    ScheduleRunId = x.ScheduleRunId,
                    ComputingJobId = x.ComputingJobId,
                    Time = x.Time,
                    Score = x.ScoreSum / x.ScoreCount
                }));
            }

            return intermediateResults.GroupBy(x => new { x.ControlId }).SelectMany(g =>
            {
                var orderedSeq = g.OrderByDescending(x => x.Time);
                return recordLatestCounts.HasValue ? orderedSeq.Take(recordLatestCounts.Value) : orderedSeq;
            });
        }
    }

    public async Task<IEnumerable<DHScoreAggregatedByControlGroup>> QueryScoreGroupByControlGroup(IEnumerable<string> dqControlGroupIds, IEnumerable<string> controlGroupIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
    {
        var dqControlGroupIdsToQuery = dqControlGroupIds.Intersect(controlGroupIds);
        var nonDqControlGroupIdsToQuery = controlGroupIds.Except(dqControlGroupIds);

        var tasks = new List<Task<IEnumerable<DHScoreAggregatedByControlGroup>>>();
        if (dqControlGroupIdsToQuery.Any())
        {
            tasks.Add(this.QueryDQScoreGroupByControlGroup(dqControlGroupIdsToQuery, domainIds, recordLatestCounts, start, end, status));
        }
        if (nonDqControlGroupIdsToQuery.Any())
        {
            tasks.Add(this.QueryNonDQScoreGroupByControlGroup(nonDqControlGroupIdsToQuery, domainIds, recordLatestCounts, start, end, status));
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        return results.SelectMany(x => x);
    }

    private async Task<IEnumerable<DHScoreAggregatedByControlGroup>> QueryDQScoreGroupByControlGroup(IEnumerable<string> controlGroupIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
    {
        // Construct the SQL query
        var sqlQuery = new StringBuilder(@$"
SELECT 
    c.ControlGroupId,
    c.ScheduleRunId,
    MAX(c.Time) AS Time,
    SUM(c.ScoreSum) as ScoreSum,
    SUM(c.ScoreCount) AS ScoreCount
FROM c WHERE c.ControlGroupId = c.ControlId AND c.AccountId = '{this.AccountIdentifier.AccountId}' ");

        // Add filtering conditions based on provided parameters
        if (domainIds != null && domainIds.Any())
        {
            // Assuming domainIds is a validated and sanitized list of IDs
            var domainIdFilter = string.Join(",", domainIds.Select(id => $"'{id}'"));
            // TODO: When added more supported artifact types, modify the SQL here.
            sqlQuery.Append($"AND ARRAY_CONTAINS([{domainIdFilter}], c.DataProductDomainId) ");
        }

        // Assuming controlIds is a validated and sanitized list of IDs
        var controlGroupIdFilter = string.Join(",", controlGroupIds.Select(id => $"'{id}'"));
        sqlQuery.Append($"AND ARRAY_CONTAINS([{controlGroupIdFilter}], c.ControlGroupId) ");

        if (start != null)
        {
            sqlQuery.Append($"AND c.Time >= '{start.Value.ToString("o")}' ");
        }

        if (end != null)
        {
            sqlQuery.Append($"AND c.Time <= '{end.Value.ToString("o")}' ");
        }

        if (status != null)
        {
            sqlQuery.Append($"AND c.DataProductStatus = '{status}' ");
        }

        sqlQuery.Append("GROUP BY c.ControlGroupId, c.ScheduleRunId");

        var queryDefinition = new QueryDefinition(sqlQuery.ToString());

        // Execute the query
        var queryResultSetIterator = this.CosmosContainer.GetItemQueryIterator<DHScoreSQLQueryResponse3>(queryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });
        var intermediateResults = new List<DHScoreSQLQueryResponse3>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var response = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
            this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response, queryDefinition.QueryText);
            intermediateResults.AddRange(response.Resource);
        }

        return intermediateResults
            .GroupBy(x => new { x.ControlGroupId, x.ScheduleRunId })
            .Where(g => g.Sum(x => x.ScoreCount) > 0)
            .Select(g => new DHScoreAggregatedByControlGroup
            {
                ControlGroupId = g.Key.ControlGroupId,
                ScheduleRunId = g.Key.ScheduleRunId,
                Time = g.Max(x => x.Time),
                Score = g.Sum(x => x.ScoreSum) / g.Sum(x => x.ScoreCount)
            })
            .GroupBy(x => new { x.ControlGroupId }).SelectMany(g =>
            {
                var orderedSeq = g.OrderByDescending(x => x.Time);
                return recordLatestCounts.HasValue ? orderedSeq.Take(recordLatestCounts.Value) : orderedSeq;
            });
    }

    private async Task<IEnumerable<DHScoreAggregatedByControlGroup>> QueryNonDQScoreGroupByControlGroup(IEnumerable<string> controlGroupIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
    {
        // Construct the SQL query
        var sqlQuery = new StringBuilder(@$"
SELECT 
    c.ControlGroupId,
    c.ControlId,
    c.ScheduleRunId,
    MAX(c.Time) AS Time,
    SUM(c.ScoreSum) as ScoreSum,
    SUM(c.ScoreCount) AS ScoreCount
FROM c WHERE c.AccountId = '{this.AccountIdentifier.AccountId}' ");

        // Add filtering conditions based on provided parameters
        if (domainIds != null && domainIds.Any())
        {
            // Assuming domainIds is a validated and sanitized list of IDs
            var domainIdFilter = string.Join(",", domainIds.Select(id => $"'{id}'"));
            // TODO: When added more supported artifact types, modify the SQL here.
            sqlQuery.Append($"AND ARRAY_CONTAINS([{domainIdFilter}], c.DataProductDomainId) ");
        }

        // Assuming controlIds is a validated and sanitized list of IDs
        var controlGroupIdFilter = string.Join(",", controlGroupIds.Select(id => $"'{id}'"));
        sqlQuery.Append($"AND ARRAY_CONTAINS([{controlGroupIdFilter}], c.ControlGroupId) ");

        if (start != null)
        {
            sqlQuery.Append($"AND c.Time >= '{start.Value.ToString("o")}' ");
        }

        if (end != null)
        {
            sqlQuery.Append($"AND c.Time <= '{end.Value.ToString("o")}' ");
        }

        if (status != null)
        {
            sqlQuery.Append($"AND c.DataProductStatus = '{status}' ");
        }

        sqlQuery.Append("GROUP BY c.ControlGroupId, c.ControlId, c.ScheduleRunId");

        var queryDefinition = new QueryDefinition(sqlQuery.ToString());

        // Execute the query
        var queryResultSetIterator = this.CosmosContainer.GetItemQueryIterator<DHScoreSQLQueryResponse2>(queryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });
        var intermediateResults = new List<DHScoreSQLQueryResponse2>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var response = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
            this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response, queryDefinition.QueryText);
            intermediateResults.AddRange(response.Resource);
        }

        return intermediateResults
            .GroupBy(x => new { x.ControlGroupId, x.ScheduleRunId })
            .Where(g => g.Sum(x => x.ScoreCount) > 0)
            .Select(g => new DHScoreAggregatedByControlGroup
            {
                ControlGroupId = g.Key.ControlGroupId,
                ScheduleRunId = g.Key.ScheduleRunId,
                Time = g.Max(x => x.Time),
                Score = g.Sum(x => x.ScoreSum) / g.Sum(x => x.ScoreCount)
            })
            .GroupBy(x => new { x.ControlGroupId }).SelectMany(g =>
            {
                var orderedSeq = g.OrderByDescending(x => x.Time);
                return recordLatestCounts.HasValue ? orderedSeq.Take(recordLatestCounts.Value) : orderedSeq;
            });
    }

    internal record DHScoreSQLQueryResponse1
    {
        public required string ControlGroupId { get; set; }
        public required string ControlId { get; set; }
        public required string ScheduleRunId { get; set; }
        public required string ComputingJobId { get; set; }
        public required DateTime Time { get; set; }
        public required double ScoreSum { get; set; }
        public required int ScoreCount { get; set; }
    }

    internal record DHScoreSQLQueryResponse2
    {
        public required string ControlGroupId { get; set; }
        public required string ControlId { get; set; }
        public required string ScheduleRunId { get; set; }
        public required DateTime Time { get; set; }
        public required double ScoreSum { get; set; }
        public required int ScoreCount { get; set; }
    }

    internal record DHScoreSQLQueryResponse3
    {
        public required string ControlGroupId { get; set; }
        public required string ScheduleRunId { get; set; }
        public required DateTime Time { get; set; }
        public required double ScoreSum { get; set; }
        public required int ScoreCount { get; set; }
    }
}