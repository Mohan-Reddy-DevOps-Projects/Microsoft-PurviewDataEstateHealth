// <copyright file="DHStatusPaletteController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Newtonsoft.Json;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/monitoring")]
public class DHMonitoringController(
    DHMonitoringService monitoringService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
    IAccountExposureControlConfigProvider exposureControl) : DataPlaneController
{

    [HttpPost]
    [Route("query")]
    public async Task<ActionResult> QueyMonitoringAsync([FromBody] QueryMonitoringRequest queryMonitoringRequest)
    {
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();
        // currently this is a public api, only used by tips.
        if (!exposureControl.IsDataGovHealthScheduleTriggerEnabled(accountId, string.Empty, tenantId))
        {
            logger.LogInformation($"Not allowed to get monitoring. Account id: {accountId}. Tenant id: {tenantId}.");
            return this.Unauthorized();
        }
        logger.LogInformation($"start to query jobs for control with Id: {queryMonitoringRequest.ControlId}, startTime:{queryMonitoringRequest.StartTime}, endTime: {queryMonitoringRequest.EndTime}");
        var batchResults = await monitoringService.QueryJobsWithControlId(queryMonitoringRequest.ControlId, queryMonitoringRequest.StartTime, queryMonitoringRequest.EndTime).ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }
}

public record QueryMonitoringRequest
{
    [JsonProperty("controlId")]
    public required string ControlId { get; set; }

    [JsonProperty("startTime")]
    public required DateTime StartTime { get; set; }

    [JsonProperty("endTime")]
    public required DateTime EndTime { get; set; }
}