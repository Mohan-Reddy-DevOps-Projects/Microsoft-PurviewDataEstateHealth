namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHActionRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonRepository<DataHealthActionWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHAction";

    private string DatabaseName => configuration["cosmosDb:actionDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHAction is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<List<DataHealthActionWrapper>> GetActionsByFilterAsync(CosmosDBQuery<ActionsFilter> query)
    {
        var filter = query.Filter;

        Func<DataHealthActionWrapper, bool> StatusFilter = item => filter?.Status?.Contains(item.Status) ?? true;

        var queryableLinq = this.CosmosContainer.GetItemLinqQueryable<DataHealthActionWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey }
        ).Where(item => true);

        if (filter != null)
        {
            if (filter.Status != null && filter.Status.Count > 0)
            {
                queryableLinq = queryableLinq.Where(item => filter.Status.Contains(item.Status));
            }

            if (filter.AssignedTo != null && filter.AssignedTo.Count > 0)
            {
                queryableLinq = queryableLinq.Where(item => item.AssignedTo.Intersect(filter.AssignedTo).Any());
            }

            if (filter.FindingTypes != null && filter.FindingTypes.Count > 0)
            {
                queryableLinq = queryableLinq.Where(item => filter.FindingTypes.Contains(item.FindingType));
            }

            if (filter.FindingSubTypes != null && filter.FindingSubTypes.Count > 0)
            {
                queryableLinq = queryableLinq.Where(item => filter.FindingSubTypes.Contains(item.FindingSubType));
            }

            if (filter.FindingNames != null && filter.FindingNames.Count > 0)
            {
                queryableLinq = queryableLinq.Where(item => filter.FindingNames.Contains(item.FindingName));
            }

            if (filter.Severity != null)
            {
                queryableLinq = queryableLinq.Where(item => filter.Severity == item.Severity);
            }

            if (filter.TargetEntityType != null)
            {
                queryableLinq = queryableLinq.Where(item => filter.TargetEntityType == item.TargetEntityType);
            }

            if (filter.TargetEntityIds != null && filter.TargetEntityIds.Count > 0)
            {
                queryableLinq = queryableLinq.Where(item => filter.TargetEntityIds.Contains(item.TargetEntityId));
            }

            if (filter.CreateTimeRange != null)
            {
                queryableLinq = queryableLinq.Where(item => item.SystemInfo.CreatedAt >= (filter.CreateTimeRange.Start ?? DateTime.MinValue) &&
                                                            item.SystemInfo.CreatedAt <= (filter.CreateTimeRange.End ?? DateTime.MaxValue));
            }
        }

        var feedIterator = queryableLinq.ToFeedIterator();

        var results = new List<DataHealthActionWrapper>();

        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }

        return results;
    }

    public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(CosmosDBQuery<ActionsFilter> query, string groupBy)
    {
        var actions = await this.GetActionsByFilterAsync(query).ConfigureAwait(false);
        return GroupedActions.ToGroupedActions(groupBy, actions);
    }
}
