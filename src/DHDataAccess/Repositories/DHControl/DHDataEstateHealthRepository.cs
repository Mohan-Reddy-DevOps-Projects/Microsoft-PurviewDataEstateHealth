namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class cosmosEntity
{
    public string Id { get; set; } = "";
    public string accountId { get; set; } = "";

}

public class DHDataEstateHealthRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
     CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DHControlBaseWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    //private const string ContainerName = "DHControl";

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private string DatabaseName => configuration["cosmosDb:dehDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for data estate health is not found in the configuration");


    public string ContainerName = "";

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(this.ContainerName);

    private readonly IDataEstateHealthRequestLogger logger = logger;



    public async Task<IEnumerable<cosmosEntity>> QueryControlNodesAsync(cosmosEntity filter)
    {
        var methodName = nameof(QueryControlNodesAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, ContainerName = {this.ContainerName}, tenantId = {base.TenantId}"))
        {
            try
            {
                var query = this.CosmosContainer.GetItemLinqQueryable<cosmosEntity>(
                    requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey });
                
                var resultQuery = query.ToFeedIterator();

                var results = new List<cosmosEntity>();
                while (resultQuery.HasMoreResults)
                {
                    var response = await resultQuery.ReadNextAsync().ConfigureAwait(false);
                    this.cosmosMetricsTracker.LogCosmosMetrics(this.TenantId, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, ContainerName = {this.ContainerName}, tenantId = {base.TenantId}", ex);
                throw;
            }

        }
    }

    public async Task<IEnumerable<DHControlGroupWrapper>> QueryControlGroupsAsync(TemplateFilters filter)
    {
        var methodName = nameof(QueryControlGroupsAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, tenantId = {base.TenantId}"))
        {
            try
            {
                var query = this.CosmosContainer.GetItemLinqQueryable<DHControlGroupWrapper>(
                    requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
                    .Where(x => x.Type == DHControlBaseWrapperDerivedTypes.Group);

                if (!string.IsNullOrWhiteSpace(filter?.TemplateName))
                {
                    query = query.Where(x => x.SystemTemplate == filter.TemplateName);
                }

                if (filter?.TemplateEntityIds?.Any() == true)
                {
                    query = query.Where(x => filter.TemplateEntityIds.Contains(x.SystemTemplateEntityId));
                }

                var resultQuery = query.ToFeedIterator();

                var results = new List<DHControlGroupWrapper>();
                while (resultQuery.HasMoreResults)
                {
                    var response = await resultQuery.ReadNextAsync().ConfigureAwait(false);
                    this.cosmosMetricsTracker.LogCosmosMetrics(this.TenantId, response);
                    results.AddRange([.. response]);
                }

                return results;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, tenantId = {base.TenantId}", ex);
                throw;
            }
        }
    }
}
