#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHControl;

using Asp.Versioning;
using AspNetCore.Authentication.Certificate;
using IdentityModel.S2S.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DHControlSchedule)]
[Authorize(
    Policy = "ClientCertOnly",
    AuthenticationSchemes = S2SAuthenticationDefaults.AuthenticationScheme + "," + CertificateAuthenticationDefaults.AuthenticationScheme
)]
[Route("/internal/control")]
public class InternalDHControlController(
    DHScheduleService dhScheduleService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
    IHttpContextAccessor httpContextAccessor,
    IAccountExposureControlConfigProvider exposureControl) : Controller
{
    [HttpPost]
    [Route("triggerScheduleJobCallback")]
    public async Task<ActionResult> TriggerScheduleJobRunCallback([FromBody] DHScheduleCallbackPayload requestBody)
    {

        if (requestBody == null)
        {
            return this.BadRequest();
        }

        var tenantId = requestHeaderContext.TenantId.ToString();
        var accountId = requestHeaderContext.AccountObjectId.ToString();

        logger.LogInformation("API triggerScheduleJobCallback should not be used.");
        var headers = httpContextAccessor?.HttpContext?.Request?.Headers;
        var scheduleId = headers.GetFirstOrDefault("x-dgschedule-schedule-id");
        var workflowId = headers.GetFirstOrDefault("x-dgschedule-workflow-id");
        var workflowLocation = headers.GetFirstOrDefault("x-dgschedule-workflow-location");
        logger.LogInformation($"Logic app callback information. TenantId: {tenantId}. AccountId: {accountId}. ScheduleId: {scheduleId}. WorkflowId: {workflowId}. WorkflowLocation: {workflowLocation}.");

        requestBody.Operator = DHScheduleCallbackPayload.DGScheduleServiceOperatorName;

        if (exposureControl.IsDataGovHealthScheduleQueueEnabled(accountId, null, tenantId))
        {
            await dhScheduleService.EnqueueScheduleAsync(requestBody, tenantId, accountId).ConfigureAwait(false);
        }
        else
        {
            using (logger.LogElapsed($"Schedule job callback start. TenantId: {tenantId}. AccountId: {accountId}."))
            {
                await dhScheduleService.TriggerScheduleJobCallbackAsync(requestBody).ConfigureAwait(false);
            }
        }
        return this.Ok();
    }

    [HttpPost]
    [Route("triggerScheduleJob")]
    public async Task<ActionResult> TriggerScheduleJobRun([FromBody] DHScheduleCallbackPayload requestBody)
    {
        if (requestBody == null)
        {
            return this.BadRequest();
        }
        var tenantId = requestHeaderContext.TenantId;
        var accountId = requestHeaderContext.AccountObjectId;
        using (logger.LogElapsed($"Trigger schedule job. TenantId: {tenantId}. AccountId: {accountId}."))
        {
            try
            {
                await dhScheduleService.TriggerScheduleJobCallbackAsync(requestBody).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.Message);
            }
        }
        return this.Ok();
    }

    [HttpPost]
    [Route("createDataQualitySpec")]
    public async Task<ActionResult<string>> CreateDataQualitySpec([FromBody] DHScheduleCallbackPayload requestBody)
    {
        var tenantId = requestHeaderContext.TenantId;
        var accountId = requestHeaderContext.AccountObjectId;
        using (logger.LogElapsed($"Create data quality specification. TenantId: {tenantId}. AccountId: {accountId}."))
        {
            try
            {
                string scheduleRunId = await dhScheduleService.CreateDqRulesSpecification(requestBody)
                    .ConfigureAwait(false);
                return this.Ok(scheduleRunId);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.Message);
            }
        }
    }

    [HttpPost]
    [Route("triggerMDQJobCallback")]
    public async Task<ActionResult> TriggerMDQJobCallback([FromBody] DHControlMDQJobCallbackPayload requestBody)
    {
        if (requestBody == null)
        {
            return this.BadRequest();
        }
        if (requestBody.ParseJobStatus() == DHComputingJobStatus.Unknown)
        {
            logger.LogInformation($"Invalid job status. Job Id: {requestBody.DQJobId}");
            return this.BadRequest("Invalid job status");
        }
        using (logger.LogElapsed($"MDQ job callback start. TenantId: {requestHeaderContext.TenantId}. AccountId: {requestHeaderContext.AccountObjectId}. Job Id: {requestBody.DQJobId}. Job Status: {requestBody.JobStatus}."))
        {
            try
            {
                await dhScheduleService.UpdateMDQJobStatusAsync(requestBody).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.Message);
            }
        }
        return this.Ok();
    }

    [HttpPost]
    [Route("upsertMdqActions")]
    public async Task<ActionResult> UpsertMdqActions([FromBody] DHControlMDQJobCallbackPayload requestBody)
    {
        if (requestBody.ParseJobStatus() == DHComputingJobStatus.Unknown)
        {
            logger.LogInformation($"Invalid job status. Job Id: {requestBody.DQJobId}");
            return this.BadRequest("Invalid job status");
        }

        using (logger.LogElapsed($"MDQ job callback start. TenantId: {requestHeaderContext.TenantId}. AccountId: {requestHeaderContext.AccountObjectId}. " +
                                 $"Job Id: {requestBody.DQJobId}. Job Status: {requestBody.JobStatus}."))
        {
            try
            {
                logger.LogInformation($"New controls flow enabled - calling UpsertMdqActions for Job Id: {requestBody.DQJobId}");
                await dhScheduleService.UpsertMdqActions(requestBody)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return this.Problem(ex.Message);
            }
        }

        return this.Ok();
    }

    [HttpPost]
    [Route("migrateSchedule")]
    public async Task<ActionResult> MigrateSchedule()
    {

        var tenantId = requestHeaderContext.TenantId;
        var accountId = requestHeaderContext.AccountObjectId;
        using (logger.LogElapsed($"Migrate schedule. TenantId: {tenantId}. AccountId: {accountId}."))
        {
            try
            {
                await dhScheduleService.MigrateScheduleAsync().ConfigureAwait(false);
                return this.Ok();
            }
            catch (Exception ex)
            {
                logger.LogError("Exception occurs when batch migrating schedule.", ex);
                return this.StatusCode(500, "Internal server error occurred while migrating schedule.");
            }
        }
    }
}
