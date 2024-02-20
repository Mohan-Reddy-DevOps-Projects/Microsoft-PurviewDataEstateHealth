namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
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

    public async Task<DataHealthActionWrapper?> GetActionByFilterAsync(DataHealthActionCategory category, string findingType, string findingSubType, string findingId, DataHealthActionTargetEntityType targetType, string targetId)
    {
        var query = this.CosmosContainer.GetItemLinqQueryable<DataHealthActionWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey }
        ).Where(
            x => x.Category == category &&
            x.FindingType == findingType &&
            x.FindingSubType == findingSubType &&
            x.FindingId == findingId &&
            x.TargetEntityType == targetType &&
            x.TargetEntityId == targetId).ToFeedIterator();

        if (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync().ConfigureAwait(false);
            return response.FirstOrDefault(); // Using FirstOrDefault to get a single item or null
        }

        return null;
    }

    public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(string groupBy)
    {
        // TODO: need a more efficient way
        var actions = await this.GetAllAsync().ConfigureAwait(false);
        return GroupedActions.ToGroupedActions(groupBy, actions);
    }
}
