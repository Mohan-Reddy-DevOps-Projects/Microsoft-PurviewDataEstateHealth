// <copyright file="DHStatusPaletteController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/schedule")]
public class DHScheduleController(DHScheduleService scheduleService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> GetGlobalScheduleAsync()
    {
        var result = await scheduleService.GetGlobalScheduleAsync().ConfigureAwait(false);
        return this.Ok(result.JObject);
    }

    [HttpPost]
    [Route("trigger")]
    public async Task<ActionResult> TriggerScheduleAsync()
    {
        await scheduleService.TriggerScheduleAsync().ConfigureAwait(false);
        return this.Ok(new Dictionary<string, bool>() { { "succeeded", true } });
    }

    [HttpPut]
    [Route("")]
    public async Task<ActionResult> CreateOrUpdateGlobalScheduleAsync(
    [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = new DHControlGlobalSchedulePayloadWrapper(payload);
        var result = await scheduleService.CreateOrUpdateGlobalScheduleAsync(entity).ConfigureAwait(false);
        return this.Ok(result.JObject);
    }
}
