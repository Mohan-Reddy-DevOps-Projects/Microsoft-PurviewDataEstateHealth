// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------
#nullable enable

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/actions")]
public class DHActionController(DHActionService actionService) : DataPlaneController
{
    [HttpPost]
    [Route("query")]
    public async Task<ActionResult> ListActionsAsync(
        [FromQuery] string? domainId = null)
    {

        // TODO(Han): Support filters
        var results = await actionService.EnumerateActionsAsync().ConfigureAwait(false);

        var batchResults = new BatchResults<DataHealthActionWrapper>(results, results.Count());

        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("grouped")]
    public async Task<ActionResult> ListActionsByGroupAsync(
        [FromBody] ActionGroupedRequest payload)
    {
        // TODO(Han): Support filters
        var results = await actionService.EnumerateActionsByGroupAsync(payload.groupBy).ConfigureAwait(false);

        return this.Ok(results);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateActionAsync(
        [FromBody] JObject payload)
    {
        var wrapper = DataHealthActionWrapper.Create(payload);
        var entity = await actionService.CreateActionsAsync(wrapper).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{entity.Id}"), entity.JObject);
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

}
