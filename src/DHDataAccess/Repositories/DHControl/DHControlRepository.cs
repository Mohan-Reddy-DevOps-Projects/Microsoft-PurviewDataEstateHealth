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

public class DHControlRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
     CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DHControlBaseWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    private const string ContainerName = "DHControl";

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    private readonly IDataEstateHealthRequestLogger logger = logger;

    public async Task<IEnumerable<DHControlNodeWrapper>> QueryControlNodesAsync(ControlNodeFilters filter)
    {
        var methodName = nameof(QueryControlNodesAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, {this.AccountIdentifier.Log}"))
        {
            try
            {
                var query = this.CosmosContainer.GetItemLinqQueryable<DHControlNodeWrapper>(
                    requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
                    .Where(x => x.Type == DHControlBaseWrapperDerivedTypes.Node && x.AccountId == this.AccountIdentifier.AccountId);

                if (filter?.AssessmentIds?.Any() == true)
                {
                    query = query.Where(x => filter.AssessmentIds.Contains(x.AssessmentId ?? string.Empty));
                }

                if (!string.IsNullOrWhiteSpace(filter?.StatusPaletteId))
                {
                    query = query.Where(x =>
                    x.StatusPaletteConfig != null &&
                    (
                        x.StatusPaletteConfig.FallbackStatusPaletteId == filter.StatusPaletteId ||
                        (
                            x.StatusPaletteConfig.StatusPaletteRules != null &&
                            x.StatusPaletteConfig.StatusPaletteRules.Select(y => y.StatusPaletteId).Contains(filter.StatusPaletteId)
                        )
                    )
                    );
                }

                if (filter?.ParentControlIds?.Any() == true)
                {
                    query = query.Where(x => filter.ParentControlIds.Contains(x.GroupId ?? string.Empty));
                }

                if (!string.IsNullOrWhiteSpace(filter?.TemplateName))
                {
                    query = query.Where(x => x.SystemTemplate == filter.TemplateName);
                }

                if (filter?.TemplateEntityIds?.Any() == true)
                {
                    query = query.Where(x => filter.TemplateEntityIds.Contains(x.SystemTemplateEntityId));
                }

                var resultQuery = query.ToFeedIterator();

                var results = new List<DHControlNodeWrapper>();
                while (resultQuery.HasMoreResults)
                {
                    var response = await resultQuery.ReadNextAsync().ConfigureAwait(false);
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

    public async Task<IEnumerable<DHControlGroupWrapper>> QueryControlGroupsAsync(TemplateFilters filter)
    {
        var methodName = nameof(QueryControlGroupsAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, {this.AccountIdentifier.Log}"))
        {
            try
            {
                var query = this.CosmosContainer.GetItemLinqQueryable<DHControlGroupWrapper>(
                    requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
                    .Where(x => x.Type == DHControlBaseWrapperDerivedTypes.Group && x.AccountId == this.AccountIdentifier.AccountId);

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
}
