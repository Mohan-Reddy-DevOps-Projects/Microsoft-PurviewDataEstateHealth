namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
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
    IDataEstateHealthRequestLogger logger)
    : CommonHttpContextRepository<DHScoreBaseWrapper>(requestHeaderContext, logger)
{
    private const string ContainerName = "DHScore";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Azure.Cosmos.Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<IEnumerable<DHScoreAggregatedByControl>> QueryScoreGroupByControl(IEnumerable<string> controlIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
    {
        // Construct the SQL query
        var sqlQuery = new StringBuilder("SELECT c.ControlId, c.ComputingJobId, MAX(c.Time) AS Time, AVG(c.AggregatedScore) AS Score FROM c WHERE 1=1 ");

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

        sqlQuery.Append("GROUP BY c.ControlId, c.ComputingJobId");

        var queryDefinition = new QueryDefinition(sqlQuery.ToString());

        // Execute the query
        var queryResultSetIterator = this.CosmosContainer.GetItemQueryIterator<DHScoreAggregatedByControl>(queryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });
        var intermediateResults = new List<DHScoreAggregatedByControl>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
            intermediateResults.AddRange(currentResultSet.Resource);
        }

        return intermediateResults.GroupBy(x => new { x.ControlId }).SelectMany(g =>
        {
            var orderedSeq = g.OrderByDescending(x => x.Time);
            return recordLatestCounts.HasValue ? orderedSeq.Take(recordLatestCounts.Value) : orderedSeq;
        });
    }

    public async Task<IEnumerable<DHScoreAggregatedByControlGroup>> QueryScoreGroupByControlGroup(IEnumerable<string> controlGroupIds, IEnumerable<string>? domainIds, int? recordLatestCounts, DateTime? start, DateTime? end, string? status)
    {
        // Construct the SQL query
        var sqlQuery = new StringBuilder(@$"
SELECT 
    c.ControlGroupId,
    SUBSTRING(c.Time, 0, 10) AS Time,
    AVG(c.AggregatedScore) AS Score
FROM c WHERE 1=1 ");

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

        sqlQuery.Append("GROUP BY c.ControlGroupId, SUBSTRING(c.Time, 0, 10)");

        var queryDefinition = new QueryDefinition(sqlQuery.ToString());

        // Execute the query
        var queryResultSetIterator = this.CosmosContainer.GetItemQueryIterator<DHScoreAggregatedByControlGroup>(queryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });
        var intermediateResults = new List<DHScoreAggregatedByControlGroup>();

        while (queryResultSetIterator.HasMoreResults)
        {
            var currentResultSet = await queryResultSetIterator.ReadNextAsync().ConfigureAwait(false);
            intermediateResults.AddRange(currentResultSet.Resource);
        }

        return intermediateResults.GroupBy(x => new { x.ControlGroupId }).SelectMany(g =>
        {
            var orderedSeq = g.OrderByDescending(x => x.Time);
            return recordLatestCounts.HasValue ? orderedSeq.Take(recordLatestCounts.Value) : orderedSeq;
        });
    }
}