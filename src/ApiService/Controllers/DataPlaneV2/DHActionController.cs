// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------
#nullable enable

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Attributes;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.Shared;
using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Newtonsoft.Json.Linq;
using System.Globalization;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/actions")]
public class DHActionController(DHActionService actionService) : DataPlaneController
{
    [HttpPost]
    [Route("query")]
    public async Task<ActionResult> ListActionsAsync(
        [FromBody] ActionQueryRequest payload)
    {
        var query = new CosmosDBQuery<ActionsFilter>()
        {
            Filter = BuildActionFilter(payload.Filters)
        };
        var results = await actionService.EnumerateActionsAsync(query).ConfigureAwait(false);

        var batchResults = new BatchResults<DataHealthActionWrapper>(results, results.Count());

        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("grouped")]
    public async Task<ActionResult> ListActionsByGroupAsync(
        [FromBody] ActionGroupedRequest payload)
    {
        var query = new CosmosDBQuery<ActionsFilter>()
        {
            Filter = BuildActionFilter(payload.Filters)
        };

        var results = await actionService.EnumerateActionsByGroupAsync(query, payload.groupBy).ConfigureAwait(false);

        return this.Ok(results);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateActionAsync(
        [FromBody] JArray payload)
    {
        var actionWrapperList = payload.Select(item => DataHealthActionWrapper.Create((JObject)item)).ToList();

        var entites = await actionService.CreateActionsAsync(actionWrapperList).ConfigureAwait(false);
        return this.Ok(entites.Select((item) => item.JObject));
    }

    [HttpGet]
    [Route("{actionId}")]
    public async Task<ActionResult> GetActionByIdAsync(string actionId)
    {
        var entity = await actionService.GetActionByIdAsync(actionId).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    [HttpPut]
    [Route("{actionId}")]
    public async Task<ActionResult> UpdateActionAsync(string actionId, [FromBody] JObject payload)
    {
        var wrapper = DataHealthActionWrapper.Create(payload);
        var entity = await actionService.UpdateActionAsync(actionId, wrapper).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    [HttpPost]
    [Route("facets")]
    public async Task<ActionResult> GetActionFacetsAsync(
    [FromBody] ActionFacetRequest payload)
    {
        var facets = new ActionFacets();

        var filters = BuildActionFilter(payload.Filters);

        var availableFacets = typeof(ActionFacets).GetProperties().Select((p) => (p, p.GetCustomAttributes(typeof(FacetAttribute), false).FirstOrDefault() as FacetAttribute)).Where((p) => p.Item2 != null).ToList();

        // TODO(han): using a mapping between facet names and their corresponding properties instead of reflection? 
        payload.Facets?.ForEach((facet) =>
        {
            var facetPeoperty = availableFacets.FirstOrDefault((f) => string.Equals(f.Item2?.FacetName, facet.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (facetPeoperty.Item2 == null)
            {
                throw new InvalidRequestException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageInvalidFacet, facet.Name, string.Join(", ", availableFacets.Select((f) => f.Item2?.FacetName))));
            }

            typeof(ActionFacets).GetProperty(facetPeoperty.p.Name)?.SetValue(facets, new FacetEntity(true));
        });

        var results = await actionService.GetActionFacetsAsync(filters, facets).ConfigureAwait(false);

        var response = new FacetResponse();

        availableFacets.ForEach(facetPeoperty =>
        {
            var facetValue = (FacetEntity?)facetPeoperty.p.GetValue(facets);

            if (facetValue?.IsEnabled == true && facetPeoperty.Item2?.FacetName != null)
            {
                response.Facets.Add(facetPeoperty.Item2.FacetName, facetValue?.Items?.Select((item) => new FacetResponseObject(item.Value, item.Count)).ToList());
            }
        });


        return this.Ok(response);
    }

    private static ActionsFilter BuildActionFilter(ActionFiltersPayload filters)
    {
        var statusFilter = new List<DataHealthActionStatus>();
        if (filters?.Status?.Count > 0)
        {
            foreach (var statusStr in filters.Status)
            {
                if (Enum.TryParse<DataHealthActionStatus>(statusStr, true, out var result))
                {
                    statusFilter.Add(result);
                }
            }
        }
        return new ActionsFilter()
        {
            DomainIds = filters?.DomainIds,
            FindingTypes = filters?.FindingTypes,
            FindingSubTypes = filters?.FindingSubTypes,
            FindingNames = filters?.FindingNames,
            Status = statusFilter,
            TargetEntityType = Enum.TryParse<DataHealthActionTargetEntityType>(filters?.TargetEntityType, true, out var targetEntityType) ? targetEntityType : null,
            TargetEntityIds = filters?.TargetEntityIds,
            AssignedTo = filters?.AssignedTo,
            Severity = Enum.TryParse<DataHealthActionSeverity>(filters?.Severity, true, out var severity) ? severity : null,
            CreateTimeRange =
                    new CreateTimeRangeFilter()
                    {
                        Start = filters?.CreateTimeRange?.Start ?? null,
                        End = filters?.CreateTimeRange?.End ?? null,
                    }
        };
    }
}
