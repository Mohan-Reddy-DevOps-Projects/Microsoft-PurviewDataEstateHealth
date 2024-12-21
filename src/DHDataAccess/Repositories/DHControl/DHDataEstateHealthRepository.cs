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
    IDefaultCosmosClient defaultCosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
     CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DHControlBaseWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    //private const string ContainerName = "DHControl";
    
    private const string BusinessDomainContainerName = "businessdomain";
    
    private Container BusinessDomainContainer => cosmosClient
        .GetDatabase(this.DatabaseName)
        .GetContainer(BusinessDomainContainerName);

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private string DatabaseName => configuration["cosmosDb:dehDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for data estate health is not found in the configuration");


    public string ContainerName = "";

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(this.ContainerName);

    private readonly IDataEstateHealthRequestLogger logger = logger;
    
    public async Task<IEnumerable<CosmosEntity>> QueryControlNodesAsync(CosmosEntity filter)
    {
        var methodName = nameof(QueryControlNodesAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, ContainerName = {this.ContainerName}, {this.AccountIdentifier.Log}"))
        {
            try
            {
                var query = this.CosmosContainer.GetItemLinqQueryable<CosmosEntity>(
                    requestOptions: new QueryRequestOptions { PartitionKey = base.TenantPartitionKey }).Where(x => x.AccountId == this.AccountIdentifier.AccountId);

                var resultQuery = query.ToFeedIterator();

                var results = new List<CosmosEntity>();
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
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, ContainerName = {this.ContainerName}, {this.AccountIdentifier.Log}", ex);
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
    
    /// <summary>
    /// Checks if any documents exist in the business domain container for the current account
    /// </summary>
    /// <returns>True if documents exist, false otherwise</returns>
    public async Task<bool> DoesBusinessDomainHaveDocumentsAsync()
    {
        const string methodName = nameof(this.DoesBusinessDomainHaveDocumentsAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, {this.AccountIdentifier.Log}"))
        {
            try
            {
                const string query = @"
                    SELECT VALUE COUNT(1)
                    FROM c
                    WHERE c.payloadKind = @payloadKind
                ";

                using var iterator = defaultCosmosClient.Client
                    .GetDatabase(this.DatabaseName)
                    .GetContainer(BusinessDomainContainerName)
                    .GetItemQueryIterator<bool>(
                        queryDefinition: new QueryDefinition(query)
                            .WithParameter("@payloadKind", "BusinessDomain"),
                        requestOptions: new QueryRequestOptions
                        {
                            PartitionKey = new PartitionKey(this.AccountIdentifier.AccountId), MaxItemCount = 1
                        });

                if (!iterator.HasMoreResults)
                {
                    return false;
                }

                var response = await iterator.ReadNextAsync().ConfigureAwait(false);
                this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response);
                return response.Resource.FirstOrDefault();

            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    $"{this.GetType().Name}#{methodName}: Failed checking business domain existence. " +
                    $"{this.AccountIdentifier.Log}",
                    ex);
                throw;
            }
        }
    }
}
