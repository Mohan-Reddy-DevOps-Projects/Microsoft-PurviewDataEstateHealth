namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHScoreRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonRepository<DHScoreWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHScore";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<IEnumerable<DHScoreAggregatedByControl>> Query(IEnumerable<string>? domainIds, IEnumerable<string>? controlIds, int? recordLatestCounts, DateTime? start, DateTime? end)
    {
        // Fetching filtered data from Cosmos DB
        var query = this.CosmosContainer.GetItemLinqQueryable<DHScoreWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = this.TenantPartitionKey }
        );

        if (domainIds != null && domainIds.Any())
        {
            query.Where(score => domainIds.Contains(score.EntityDomainId));
        }

        if (controlIds != null && controlIds.Any())
        {
            query.Where(score => controlIds.Contains(score.ControlId));
        }

        if (start != null)
        {
            query.Where(score => score.Time >= start);
        }

        if (end != null)
        {
            query.Where(score => score.Time <= end);
        }

        var feedIterator = query.ToFeedIterator();

        var results = new List<DHScoreWrapper>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }

        // Adjusted in-memory grouping, ordering, and aggregation logic to incorporate pointsCount
        var aggregatedResults = results
            .GroupBy(score => score.ControlId)
            .SelectMany(group => group
                // Order by Time descending to get the latest scores first
                .OrderByDescending(score => score.Time)
                // Then group by ComputingJobId to aggregate scores within the same job
                .GroupBy(score => score.ComputingJobId)
                // Take only the latest N ComputingJobIds based on pointsCount
                .Take(recordLatestCounts ?? int.MaxValue)
                .Select(subGroup => new DHScoreAggregatedByControl
                {
                    ControlId = group.Key,
                    ComputingJobId = subGroup.Key,
                    Time = subGroup.Max(score => score.Time), // Assuming the latest time in the sub-group
                    Score = subGroup.Average(score => score.AggregatedScore) // Assuming the average score as the aggregated score
                }))
            .ToList();

        return aggregatedResults;
    }
}