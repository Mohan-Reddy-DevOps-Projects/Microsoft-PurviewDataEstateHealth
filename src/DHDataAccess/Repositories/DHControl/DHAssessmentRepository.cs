namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHAssessmentRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
    CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DHAssessmentWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    private const string ContainerName = "DHAssessment";

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<IEnumerable<DHAssessmentWrapper>> QueryAssessmentsAsync(TemplateFilters filter)
    {
        var query = this.CosmosContainer.GetItemLinqQueryable<DHAssessmentWrapper>(
            requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey })
            .Where(x => true);

        if (!string.IsNullOrWhiteSpace(filter?.TemplateName))
        {
            query = query.Where(x => x.SystemTemplate == filter.TemplateName);
        }

        if (filter?.TemplateEntityIds?.Any() == true)
        {
            query = query.Where(x => filter.TemplateEntityIds.Contains(x.SystemTemplateEntityId));
        }

        var resultQuery = query.ToFeedIterator();

        var results = new List<DHAssessmentWrapper>();
        while (resultQuery.HasMoreResults)
        {
            var response = await resultQuery.ReadNextAsync().ConfigureAwait(false);
            this.cosmosMetricsTracker.LogCosmosMetrics(this.TenantId, response);
            results.AddRange([.. response]);
        }

        return results;
    }
}