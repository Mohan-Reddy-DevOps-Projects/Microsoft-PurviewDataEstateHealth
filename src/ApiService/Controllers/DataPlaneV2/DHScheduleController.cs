// <copyright file="DHStatusPaletteController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/schedule")]
public class DHScheduleController(
    DHScheduleService scheduleService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
    IAccountExposureControlConfigProvider exposureControl) : DataPlaneController
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
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();
        if (!exposureControl.IsDataGovHealthScheduleTriggerEnabled(accountId, string.Empty, tenantId))
        {
            logger.LogInformation($"Not allowed to trigger schedule. Account id: {accountId}. Tenant id: {tenantId}.");
            return this.Unauthorized();
        }
        logger.LogInformation("Manually trigger schedule start.");
        var payload = new DHScheduleCallbackPayload
        {
            Operator = requestHeaderContext.ClientObjectId,
            TriggerType = DHScheduleCallbackTriggerType.Manually
        };
        var scheduleRunId = await scheduleService.TriggerScheduleJobCallbackAsync(payload).ConfigureAwait(false);
        logger.LogInformation($"Manually trigger schedule successfully. ScheduleRunId: {scheduleRunId}. Operator: {requestHeaderContext.ClientObjectId}.");
        return this.Ok(new Dictionary<string, string>() { { "scheduleRunId", scheduleRunId } });
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
