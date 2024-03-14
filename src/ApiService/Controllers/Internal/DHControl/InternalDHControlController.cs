#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHControl;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
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
public class InternalDHControlController(DHScheduleService dhScheduleService, IDataEstateHealthRequestLogger logger, IRequestHeaderContext requestHeaderContext) : Controller
{
    [HttpPost]
    [Route("triggerScheduleJobCallback")]
    public async Task<ActionResult> TriggerScheduleJobRunCallback([FromBody] DHScheduleCallbackPayload requestBody)
    {
        if (requestBody == null)
        {
            return this.BadRequest();
        }
        logger.LogInformation($"Schedule job callback start. TenantId: {requestHeaderContext.TenantId}. AccountId: {requestHeaderContext.AccountObjectId}.");
        await dhScheduleService.TriggerScheduleJobCallbackAsync(requestBody).ConfigureAwait(false);
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
        logger.LogInformation($"MDQ job callback start. TenantId: {requestHeaderContext.TenantId}. AccountId: {requestHeaderContext.AccountObjectId}. Job Id: {requestBody.DQJobId}. Job Status: {requestBody.JobStatus}.");
        await dhScheduleService.UpdateMDQJobStatusAsync(requestBody).ConfigureAwait(false);
        return this.Ok();
    }
}
