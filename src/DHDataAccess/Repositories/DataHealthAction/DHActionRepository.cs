namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Attributes;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class DHActionRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
    CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DataHealthActionWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    private readonly IDataEstateHealthRequestLogger logger = logger;

    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;

    private const string ContainerName = "DHAction";

    private string DatabaseName => configuration["cosmosDb:actionDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHAction is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);

    public async Task<BatchResults<DataHealthActionWrapper>> GetActionsByFilterAsync(CosmosDBQuery<ActionsFilter> query, bool fetchAll = false)
    {
        using (this.logger.LogElapsed("Start to query actions in DB"))
        {
            var sqlQuery = new StringBuilder($"SELECT * FROM c WHERE c.AccountId = @accountId");
            sqlQuery.Append(this.GenerateFilterQueryStr(query.Filter));

            if (query.Sorters != null && query.Sorters.Count != 0)
            {
                sqlQuery.Append(" ORDER BY ");
                var orderByConditions = new List<string>();
                foreach (var sorter in query.Sorters)
                {
                    orderByConditions.Add($"c.{sorter.Field} {(sorter.Order == SortOrder.Ascending ? "asc" : "desc")}");
                }
                sqlQuery.Append(string.Join(", ", orderByConditions));
            }

                var sqlQueryText = sqlQuery.ToString();

                var queryDefinition = new QueryDefinition(sqlQueryText)
                    .WithParameter("@accountId", this.AccountIdentifier.AccountId);
            var feedIterator = this.CosmosContainer.GetItemQueryIterator<DataHealthActionWrapper>(
                queryDefinition,
                query.ContinuationToken,
                new QueryRequestOptions
                {
                    PartitionKey = this.TenantPartitionKey,
                    MaxItemCount = query.PageSize ?? 200,
                });

            var results = new List<DataHealthActionWrapper>();

            FeedResponse<DataHealthActionWrapper>? response = null;

            if (query.PageSize != 0)
            {
                response = await this.retryPolicy.ExecuteAsync(
                    (context) => feedIterator.ReadNextAsync(),
                    new Context($"{this.GetType().Name}#{nameof(GetActionsByFilterAsync)}_{this.AccountIdentifier.ConcatenatedId}")
                ).ConfigureAwait(false);
                this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response);
                results.AddRange(response);

                while (feedIterator.HasMoreResults && fetchAll)
                {
                    response = await this.retryPolicy.ExecuteAsync(
                        (context) => feedIterator.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{nameof(GetActionsByFilterAsync)}_{this.AccountIdentifier.ConcatenatedId}")
                    ).ConfigureAwait(false);
                    this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response);
                    results.AddRange(response);
                }
            }

            var totalCount = await this.QueryCount(query.Filter).ConfigureAwait(false);

            return new BatchResults<DataHealthActionWrapper>(
                results, totalCount, response?.ContinuationToken
            );
        };
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

                        sqlQuery.Append($"SELECT {property.Name} as 'Value', COUNT(1) as 'Count' FROM c JOIN {property.Name} IN c.{property.Name} WHERE c.AccountId = @accountId ");

                        sqlQuery.Append(this.GenerateFilterQueryStr(filters));

                        sqlQuery.Append($" GROUP BY {property.Name}");
                    }
                    else
                    {
                        sqlQuery.Append($"SELECT c.{property.Name} as 'Value', COUNT(1) as 'Count' FROM c WHERE c.AccountId = @accountId ");

                        sqlQuery.Append(this.GenerateFilterQueryStr(filters));

                        sqlQuery.Append($" GROUP BY c.{property.Name}");
                    }

                    string sqlQueryText = sqlQuery.ToString();

                        var queryDefinition = new QueryDefinition(sqlQueryText)
                            .WithParameter("@accountId", this.AccountIdentifier.AccountId);

                    FeedIterator<FacetEntityItem> sqlResultSetIterator = this.CosmosContainer.GetItemQueryIterator<FacetEntityItem>(queryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });

                    List<FacetEntityItem> sqlResults = new List<FacetEntityItem>();
                    while (sqlResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<FacetEntityItem> currentResultSet = await sqlResultSetIterator.ReadNextAsync().ConfigureAwait(false);
                        this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, currentResultSet);
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
            if (filter.DomainIds != null && filter.DomainIds.Count != 0)
            {
                var domainIds = string.Join(", ", filter.DomainIds.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.DomainId IN ({domainIds})");
            }

            if (filter.Categories != null && filter.Categories.Count != 0)
            {
                var categories = string.Join(", ", filter.Categories.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.Category IN ({categories})");
            }

            if (filter.Status != null && filter.Status.Count != 0)
            {
                var statuses = string.Join(", ", filter.Status.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.Status IN ({statuses})");
            }

            if (filter.AssignedTo != null && filter.AssignedTo.Count != 0)
            {
                var assignedToConditions = filter.AssignedTo.Select(assignedTo => $"ARRAY_CONTAINS(c.AssignedTo, '{assignedTo}')");
                var assignedToQuery = string.Join(" OR ", assignedToConditions);
                sqlQuery.Append($" AND ({assignedToQuery})");
            }

            if (filter.FindingTypes != null && filter.FindingTypes.Count != 0)
            {
                var findingTypes = string.Join(", ", filter.FindingTypes.Select(x => EscapStr(x)));
                sqlQuery.Append($" AND c.FindingType IN ({findingTypes})");
            }

            if (filter.FindingSubTypes != null && filter.FindingSubTypes.Count != 0)
            {
                var findingSubTypes = string.Join(", ", filter.FindingSubTypes.Select(x => EscapStr(x)));
                sqlQuery.Append($" AND c.FindingSubType IN ({findingSubTypes})");
            }

            if (filter.FindingNames != null && filter.FindingNames.Count != 0)
            {
                var findingNames = string.Join(", ", filter.FindingNames.Select(x => EscapStr(x)));
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

            if (filter.TargetEntityIds != null && filter.TargetEntityIds.Count != 0)
            {
                var targetEntityIds = string.Join(", ", filter.TargetEntityIds.Select(x => $"'{x}'"));
                sqlQuery.Append($" AND c.TargetEntityId IN ({targetEntityIds})");
            }

            if (filter.CreatedTimeRange != null)
            {
                if (filter.CreatedTimeRange.Start != null)
                {
                    sqlQuery.Append($" AND c.SystemInfo.CreatedAt >= '{filter.CreatedTimeRange.Start.Value.ToString("o")}'");
                }

                if (filter.CreatedTimeRange.End != null)
                {
                    sqlQuery.Append($" AND c.SystemInfo.CreatedAt <= '{filter.CreatedTimeRange.End.Value.ToString("o")}'");
                }
            }

            if (filter.ResolvedTimeRange != null)
            {
                if (filter.ResolvedTimeRange.Start != null)
                {
                    sqlQuery.Append($" AND c.SystemInfo.ResolvedAt >= '{filter.ResolvedTimeRange.Start.Value.ToString("o")}'");
                }

                if (filter.ResolvedTimeRange.End != null)
                {
                    sqlQuery.Append($" AND c.SystemInfo.ResolvedAt <= '{filter.ResolvedTimeRange.End.Value.ToString("o")}'");
                }
            }

            if (filter.PermissionObligations != null)
            {
                var permissionObligationsConditions = new List<string>();

                foreach (var permissionObligation in filter.PermissionObligations)
                {
                    var entityType = $"'{permissionObligation.Key}'";
                    var obligations = new List<string>();

                    foreach (var obligation in permissionObligation.Value)
                    {
                        if (obligation.BusinessDomains == null || obligation.BusinessDomains.Count == 0)
                        {
                            obligation.BusinessDomains = new List<string> { "FAKE_GUID" };
                        }

                        var businessDomains = string.Join(", ", obligation.BusinessDomains.Select(x => $"'{x}'"));

                        if (obligation.Type == ObligationType.Permit)
                        {
                            obligations.Add($"(c.TargetEntityType = {entityType} AND c.DomainId IN ({businessDomains}))");
                        }
                        else if (obligation.Type == ObligationType.NotApplicable && businessDomains.Any())
                        {
                            obligations.Add($"(c.TargetEntityType = {entityType} AND c.DomainId NOT IN ({businessDomains}))");
                        }
                    }

                    if (obligations.Count != 0)
                    {
                        var obligationsQuery = string.Join(" OR ", obligations);
                        permissionObligationsConditions.Add($"({obligationsQuery})");
                    }
                }
                if (permissionObligationsConditions.Any())
                {
                    var permissionObligationsQuery = string.Join(" OR ", permissionObligationsConditions);
                    sqlQuery.Append($" AND ({permissionObligationsQuery})");
                }
            }
        }
        return sqlQuery;
    }

    public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(CosmosDBQuery<ActionsFilter> query, string groupBy)
    {
        using (this.logger.LogElapsed("Start to query grouped actions in DB"))
        {
            var actions = await this.GetActionsByFilterAsync(query, true).ConfigureAwait(false);
            return GroupedActions.ToGroupedActions(groupBy, actions.Results);
        }
    }

    private async Task<int> QueryCount(ActionsFilter? filter)
    {
        var countQuery = new StringBuilder($"SELECT VALUE COUNT(1) FROM c WHERE c.AccountId = @accountId");
        if (filter != null)
        {
            countQuery.Append(this.GenerateFilterQueryStr(filter));
        }
        var countQueryDefinition = new QueryDefinition(countQuery.ToString())
                            .WithParameter("@accountId", this.AccountIdentifier.AccountId);
        var countFeedIterator = this.CosmosContainer.GetItemQueryIterator<int>(countQueryDefinition, null, new QueryRequestOptions { PartitionKey = this.TenantPartitionKey });

        var countResponse = await countFeedIterator.ReadNextAsync().ConfigureAwait(false);
        this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, countResponse);
        return countResponse.Resource.FirstOrDefault();
    }

    private static string EscapStr(string originStr)
    {
        return $"'{originStr.Replace("'", "\\\'").Replace("\"", "\\\"")}'";
    }
}
