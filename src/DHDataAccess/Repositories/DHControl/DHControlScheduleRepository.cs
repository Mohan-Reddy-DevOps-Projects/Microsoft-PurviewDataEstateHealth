namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHControlScheduleRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonRepository<DHControlScheduleStoragePayloadWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHSchedule";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<IEnumerable<DHControlScheduleStoragePayloadWrapper>> QueryScheduleAsync(DHControlScheduleType scheduleType)
    {
        var query = this.CosmosContainer.GetItemLinqQueryable<DHControlScheduleStoragePayloadWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
            .Where(c => c.Type == scheduleType)
            .ToFeedIterator();

        var results = new List<DHControlScheduleStoragePayloadWrapper>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }

        return results;
    }
}