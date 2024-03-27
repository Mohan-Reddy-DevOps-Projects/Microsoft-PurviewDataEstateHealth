#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHControl;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using System.Threading;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DHControlSchedule)]
[Authorize(AuthenticationSchemes = "Certificate")]
[Route("/internal/control")]
public class InternalDHControlController(
    DHScheduleService dhScheduleService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
    ICoreLayerFactory coreLayerFactory) : Controller
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
        logger.LogInformation($"Schedule job callback start. TenantId: {tenantId}. AccountId: {accountId}.");
        requestBody.Operator = DHScheduleCallbackPayload.DGScheduleServiceOperatorName;
        await dhScheduleService.TriggerScheduleJobCallbackAsync(requestBody).ConfigureAwait(false);

        logger.LogInformation($"Refresh powerBI after MDQ jobs triggered. TenantId: {tenantId}. AccountId: {accountId}.");
        if (requestBody.ControlId == null)
        {
            var account = new AccountServiceModel(id: accountId.ToString(), tenantId: tenantId.ToString());
            coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                .CreatePartnerNotificationComponent(tenantId, accountId);
            await coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                .CreateDHControlTriggerComponent(tenantId, accountId)
                .RefreshPowerBI(account, CancellationToken.None);
        }
        else
        {
            logger.LogInformation($"Ignore powerBI refresh since this request is triggered by specific control {requestBody.ControlId}");
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
        logger.LogInformation($"MDQ job callback start. TenantId: {requestHeaderContext.TenantId}. AccountId: {requestHeaderContext.AccountObjectId}. Job Id: {requestBody.DQJobId}. Job Status: {requestBody.JobStatus}.");
        await dhScheduleService.UpdateMDQJobStatusAsync(requestBody).ConfigureAwait(false);
        return this.Ok();
    }
}
