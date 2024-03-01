namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Attributes;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DHActionRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger)
    : CommonHttpContextRepository<DataHealthActionWrapper>(requestHeaderContext, logger)
{
    private readonly IDataEstateHealthRequestLogger logger = logger;

    private const string ContainerName = "DHAction";

    private string DatabaseName => configuration["cosmosDb:actionDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHAction is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<List<DataHealthActionWrapper>> GetActionsByFilterAsync(CosmosDBQuery<ActionsFilter> query)
    {
        using (this.logger.LogElapsed("Start to query actions in DB"))
        {
            var sqlQuery = new StringBuilder("SELECT * FROM c WHERE 1 = 1");

            sqlQuery.Append(this.GenerateFilterQueryStr(query.Filter));

            var sqlQueryText = sqlQuery.ToString();

            var queryDefinition = new QueryDefinition(sqlQueryText);
            var feedIterator = this.CosmosContainer.GetItemQueryIterator<DataHealthActionWrapper>(queryDefinition);

            var results = new List<DataHealthActionWrapper>();

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync().ConfigureAwait(false);
                results.AddRange(response);
            }

            return results;
        }
    }

    public async Task<ActionFacets> GetActionFacetsAsync(ActionsFilter filters, ActionFacets facets)
    {
        using (this.logger.LogElapsed("Start to get action facets"))
        {
            var tasks = typeof(ActionFacets).GetProperties().Select(async property =>
            {
                var propertyValue = (FacetEntity?)property.GetValue(facets);
                if (propertyValue?.IsEnabled != true)
                {
                    this.logger.LogInformation($"Property is null, property name: {property.Name}");
                    return;
                }
                var attribute = property.GetCustomAttributes(typeof(FacetAttribute), false).FirstOrDefault() as FacetAttribute;
                if (attribute != null)
                {
                    var sqlQuery = new StringBuilder();

                    if (attribute.FacetName == DataHealthActionWrapper.keyAssignedTo)
                    {
                        this.logger.LogInformation("Query with assignedTo Facets");

                        sqlQuery.Append($"SELECT {property.Name} as 'Value', COUNT(1) as 'Count' FROM c  JOIN {property.Name} IN c.{property.Name}  WHERE 1 = 1");

                        sqlQuery.Append(this.GenerateFilterQueryStr(filters));

                        sqlQuery.Append($" GROUP BY {property.Name}");
                    }
                    else
                    {
                        sqlQuery.Append($"SELECT c.{property.Name} as 'Value', COUNT(1) as 'Count' FROM c WHERE 1 = 1");

                        sqlQuery.Append(this.GenerateFilterQueryStr(filters));

                        sqlQuery.Append($" GROUP BY c.{property.Name}");
                    }

                    string sqlQueryText = sqlQuery.ToString();

                    var queryDefinition = new QueryDefinition(sqlQueryText);

                    FeedIterator<FacetEntityItem> sqlResultSetIterator = this.CosmosContainer.GetItemQueryIterator<FacetEntityItem>(queryDefinition);

                    List<FacetEntityItem> sqlResults = new List<FacetEntityItem>();
                    while (sqlResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<FacetEntityItem> currentResultSet = await sqlResultSetIterator.ReadNextAsync().ConfigureAwait(false);
                        foreach (FacetEntityItem facetResult in currentResultSet)
                        {
                            sqlResults.Add(facetResult);
                        }
                        if (propertyValue != null)
                        {
                            propertyValue.Items = sqlResults;
                        }
                    }
                }
                else
                {
                    this.logger.LogInformation($"Attribute is null, property name: {property.Name}");
                }
            });
            await Task.WhenAll(tasks).ConfigureAwait(false);
            return facets;
        }
    }

    private StringBuilder GenerateFilterQueryStr(ActionsFilter? filter)
    {

        var sqlQuery = new StringBuilder("");

        if (filter != null)
        {
            this.logger.LogInformation("Query filter is not null");
            if (filter.DomainIds != null && filter.DomainIds.Any())
            {
                var domainIds = string.Join(", ", filter.DomainIds.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.DomainId IN ({domainIds})");
            }

            if (filter.Status != null && filter.Status.Any())
            {
                var statuses = string.Join(", ", filter.Status.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.Status IN ({statuses})");
            }

            if (filter.AssignedTo != null && filter.AssignedTo.Any())
            {
                var assignedToConditions = filter.AssignedTo.Select(assignedTo => $"ARRAY_CONTAINS(c.AssignedTo, '{assignedTo}')");
                var assignedToQuery = string.Join(" OR ", assignedToConditions);
                sqlQuery.Append($" AND ({assignedToQuery})");
            }

            if (filter.FindingTypes != null && filter.FindingTypes.Any())
            {
                var findingTypes = string.Join(", ", filter.FindingTypes.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.FindingType IN ({findingTypes})");
            }

            if (filter.FindingSubTypes != null && filter.FindingSubTypes.Any())
            {
                var findingSubTypes = string.Join(", ", filter.FindingSubTypes.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.FindingSubType IN ({findingSubTypes})");
            }

            if (filter.FindingNames != null && filter.FindingNames.Any())
            {
                var findingNames = string.Join(", ", filter.FindingNames.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.FindingName IN ({findingNames})");
            }

            if (filter.Severity != null)
            {
                sqlQuery.Append($" AND c.Severity = '{filter.Severity}'");
            }

            if (filter.TargetEntityType != null)
            {
                sqlQuery.Append($" AND c.TargetEntityType = '{filter.TargetEntityType}'");
            }

            if (filter.TargetEntityIds != null && filter.TargetEntityIds.Any())
            {
                var targetEntityIds = string.Join(", ", filter.TargetEntityIds.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.TargetEntityId IN ({targetEntityIds})");
            }

            if (filter.CreateTimeRange != null)
            {
                if (filter.CreateTimeRange.Start != null)
                {
                    sqlQuery.Append($"AND c.SystemInfo.CreatedAt >= '{filter.CreateTimeRange.Start.Value.ToString("o")}' ");
                }

                if (filter.CreateTimeRange.End != null)
                {
                    sqlQuery.Append($"AND c.SystemInfo.CreatedAt <= '{filter.CreateTimeRange.End.Value.ToString("o")}' ");
                }
            }
        }
        return sqlQuery;
    }

    public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(CosmosDBQuery<ActionsFilter> query, string groupBy)
    {
        using (this.logger.LogElapsed("Start to query grouped actions in DB"))
        {
            var actions = await this.GetActionsByFilterAsync(query).ConfigureAwait(false);
            return GroupedActions.ToGroupedActions(groupBy, actions);
        }

    }
}
