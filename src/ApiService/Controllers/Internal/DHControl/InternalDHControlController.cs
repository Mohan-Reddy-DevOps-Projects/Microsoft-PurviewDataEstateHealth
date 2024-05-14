#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHControl;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
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
[Authorize(AuthenticationSchemes = "Certificate")]
[Route("/internal/control")]
public class InternalDHControlController(
    DHScheduleService dhScheduleService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
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

        var tenantId = requestHeaderContext.TenantId;
        var accountId = requestHeaderContext.AccountObjectId;
        requestBody.Operator = DHScheduleCallbackPayload.DGScheduleServiceOperatorName;

        if (exposureControl.IsDataGovHealthScheduleQueueEnabled(accountId.ToString(), null, tenantId.ToString()))
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
            await dhScheduleService.TriggerScheduleJobCallbackAsync(requestBody).ConfigureAwait(false);
        }
        return this.Ok();
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
            await dhScheduleService.UpdateMDQJobStatusAsync(requestBody).ConfigureAwait(false);
        }
        return this.Ok();
    }
}
