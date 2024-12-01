// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Geneva action controller.
/// </summary>
[ApiController]
[Route("/controlplane/genevaAction/")]
[ApiVersionNeutral]
public class GenevaActionController : ControlPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IRequestHeaderContext requestHeaderContext;
    private readonly DHScheduleService scheduleService;

    public GenevaActionController(
        ICoreLayerFactory coreLayerFactory,
        IDataEstateHealthRequestLogger logger,
        IRequestHeaderContext requestHeaderContext,
        DHScheduleService scheduleService)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.logger = logger;
        this.requestHeaderContext = requestHeaderContext;
        this.scheduleService = scheduleService;
    }

    /// <summary>
    /// Trigger a background job registered in Azure stack.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("triggerBackgroundJob")]
    public async Task<IActionResult> TriggerBackgroundJobAsync(
        [FromBody] GenevaActionTriggerBackgroundJobRequestPayload payload,
        CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed($"Geneva action: Trigger background job. {payload.JobPartition} {payload.JobId}"))
        {
            try
            {
                await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreateDHWorkerServiceTriggerComponent(Guid.Empty, Guid.Empty)
                    .TriggerBackgroundJob(payload.JobPartition, payload.JobId, cancellationToken);
                return this.Ok();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fail to trigger background job. {payload.JobPartition} {payload.JobId}", ex);
                var response = new GenevaActionResponse { Code = "500", Message = ex.Message };
                return this.StatusCode(500, response);
            }
        }
    }


    /// <summary>
    /// Get detail of a background job registered in Azure stack.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("getBackgroundJobDetail")]
    public async Task<IActionResult> GetBackgroundJobAsync(
        [FromBody] GenevaActionGetBackgroundJobDetailRequestPayload payload)
    {
        using (this.logger.LogElapsed($"Geneva action: Get background job detail. {payload.JobPartition} {payload.JobId}"))
        {
            try
            {
                var job = await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreateDHWorkerServiceTriggerComponent(Guid.Empty, Guid.Empty)
                    .GetBackgroundJob(payload.JobPartition, payload.JobId);
                return this.Ok(job);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fail to get background job detail. {payload.JobPartition} {payload.JobId}", ex);
                var response = new GenevaActionResponse { Code = "500", Message = ex.Message };
                return this.StatusCode(500, response);
            }
        }
    }

    /// <summary>
    /// Trigger DEH schedule.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("triggerSchedule")]
    public async Task<IActionResult> TriggerScheduleAsync(
        [FromBody] GenevaActionTriggerScheduleRequestPayload payload)
    {
        var tenantId = this.requestHeaderContext.TenantId.ToString();
        var accountId = this.requestHeaderContext.AccountObjectId.ToString();
        using (this.logger.LogElapsed($"Geneva action: Trigger schedule. TenantId: {tenantId}. AccountId: {accountId}. ControlId: {payload.ControlId}"))
        {
            this.CheckAccountIdInRequestHeaders();
            var callbackPayload = new DHScheduleCallbackPayload
            {
                Operator = DHScheduleCallbackPayload.GenevaActionOperatorName,
                TriggerType = DHScheduleCallbackTriggerType.Manually,
                ControlId = payload.ControlId,
            };
            try
            {
                var scheduleRunId = await this.scheduleService.TriggerScheduleJobCallbackAsync(callbackPayload, true).ConfigureAwait(false);
                this.logger.LogInformation($"Geneva trigger schedule successfully. ScheduleRunId: {scheduleRunId}.");
                return this.Ok(new Dictionary<string, string>() { { "scheduleRunId", scheduleRunId } });
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Geneva fails to trigger schedule. TenantId: {tenantId}. AccountId: {accountId}. ControlId: {payload.ControlId}", ex);
                var response = new GenevaActionResponse { Code = "500", Message = ex.Message };
                return this.StatusCode(500, response);
            }
        }
    }

    /// <summary>
    /// List monitoring job by schedule run id.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("listMonitoringJobs")]
    public async Task<IActionResult> ListMonitoringJobsAsync(
        [FromBody] GenevaActionListMonitoringJobsRequestPayload payload)
    {
        var tenantId = this.requestHeaderContext.TenantId.ToString();
        var accountId = this.requestHeaderContext.AccountObjectId.ToString();
        var scheduleRunId = payload.ScheduleRunId;
        using (this.logger.LogElapsed($"Geneva action: list monitoring jobs. TenantId: {tenantId}. AccountId: {accountId}. ControlId: {scheduleRunId}"))
        {
            this.CheckAccountIdInRequestHeaders();
            try
            {
                var jobs = await this.scheduleService.ListMonitoringJobsByScheduleRunId(scheduleRunId).ConfigureAwait(false);
                this.logger.LogInformation($"List monitoring jobs successfully. ScheduleRunId: {scheduleRunId}. Count: {jobs.Count}.");
                return this.Ok(PagedResults.FromBatchResults(jobs));
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fail to list monitoring jobs. TenantId: {tenantId}. AccountId: {accountId}. ControlId: {scheduleRunId}", ex);
                var response = new GenevaActionResponse { Code = "500", Message = ex.Message };
                return this.StatusCode(500, response);
            }
        }
    }

    private void CheckAccountIdInRequestHeaders()
    {
        var tenantId = this.requestHeaderContext.TenantId;
        var accountId = this.requestHeaderContext.AccountObjectId;
        if (tenantId.Equals(Guid.Empty))
        {
            throw new InvalidRequestException("Empty tenant id in request headers.");
        }
        if (accountId.Equals(Guid.Empty))
        {
            throw new InvalidRequestException("Empty account id in request headers.");
        }
    }
}

