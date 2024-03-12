namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHControlRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger)
    : CommonHttpContextRepository<DHControlBaseWrapper>(requestHeaderContext, logger)
{
    private const string ContainerName = "DHControl";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<IEnumerable<DHControlNodeWrapper>> QueryControlNodesAsync(ControlNodeFilters filters)
    {
        var query = this.CosmosContainer.GetItemLinqQueryable<DHControlNodeWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
            .Where(x => x.Type == DHControlBaseWrapperDerivedTypes.Node);

        if (filters?.AssessmentIds?.Any() == true)
        {
            query = query.Where(x => filters.AssessmentIds.Contains(x.AssessmentId ?? string.Empty));
        }

        if (!string.IsNullOrWhiteSpace(filters?.StatusPaletteId))
        {
            query = query.Where(x =>
            x.StatusPaletteConfig != null &&
            (
                x.StatusPaletteConfig.FallbackStatusPaletteId == filters.StatusPaletteId ||
                (
                    x.StatusPaletteConfig.StatusPaletteRules != null &&
                    x.StatusPaletteConfig.StatusPaletteRules.Select(y => y.StatusPaletteId).Contains(filters.StatusPaletteId)
                )
            )
            );
        }

        var resultQuery = query.ToFeedIterator();

        var results = new List<DHControlNodeWrapper>();
        while (resultQuery.HasMoreResults)
        {
            var response = await resultQuery.ReadNextAsync().ConfigureAwait(false);
            results.AddRange([.. response]);
        }

        return results;
    }
}
