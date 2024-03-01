namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHComputingJobRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonHttpContextRepository<DHComputingJobWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHComputingJob";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHComputingJob is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<DHComputingJobWrapper?> GetByDQJobId(string dqJobId)
    {
        var query = this.CosmosContainer.GetItemLinqQueryable<DHComputingJobWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = this.TenantPartitionKey }
        ).Where(job => job.DQJobId == dqJobId);
        var feedIterator = query.ToFeedIterator();
        var results = new List<DHComputingJobWrapper>();
        while (feedIterator.HasMoreResults)
        {
            var response = await feedIterator.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }
        return results.FirstOrDefault();
    }
}
