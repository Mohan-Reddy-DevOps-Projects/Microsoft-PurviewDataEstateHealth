// <copyright file="DHControlController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls")]
public class DHControlController(DHControlService dataHealthControlService, DHProvisionService provisionService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListControlsAsync()
    {
        var batchResults = await dataHealthControlService.ListControlsAsync().ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateControlAsync(
    [FromBody] JObject payload,
    [FromQuery(Name = "withNewAssessment")] bool withNewAssessment)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHControlBaseWrapper.Create(payload);
        var result = await dataHealthControlService.CreateControlAsync(entity, withNewAssessment).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{result.Id}"), result.JObject);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetControlById(string id)
    {
        var entity = await dataHealthControlService.GetControlByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> UpdateControlByIdAsync(string id, [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var wrapper = DHControlBaseWrapper.Create(payload);

        var entity = await dataHealthControlService.UpdateControlByIdAsync(id, wrapper).ConfigureAwait(false);

        return this.Ok(entity.JObject);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteControlByIdAsync(string id, [FromQuery(Name = "deleteAssessment")] bool deleteAssessment)
    {
        await dataHealthControlService.DeleteControlByIdAsync(id, deleteAssessment).ConfigureAwait(false);

        return this.NoContent();
    }

    [HttpPost]
    [Route("{id}/reset")]
    public async Task<ActionResult> ResetControlByIdAsync(string id)
    {
        var entity = await provisionService.ResetControlByIdAsync(id).ConfigureAwait(false);

        return this.Ok(entity.JObject);
    }

    [HttpPost]
    [Route("reset")]
    public async Task<ActionResult> ResetAllControlsAsync()
    {
        await provisionService.ResetControlTemplate("CDMC").ConfigureAwait(false);

        return this.Ok();
    }
}
