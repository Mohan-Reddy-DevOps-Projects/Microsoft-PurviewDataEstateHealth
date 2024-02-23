#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHControl;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DHControlSchedule)]
[Authorize(AuthenticationSchemes = "Certificate")]
[Route("/internal/control")]
public class InternalDHControlController(DHScheduleService dhScheduleService, IDataEstateHealthRequestLogger logger) : Controller
{
    [HttpPost]
    [Route("triggerScheduleJobCallback")]
    public async Task<ActionResult> TriggerScheduleJobRunCallback([FromBody] DHScheduleCallbackPayload requestBody)
    {
        if (requestBody == null)
        {
            return this.BadRequest();
        }
        logger.LogInformation($"Schedule job callback start. {requestBody.TenantId} {requestBody.AccountId}");
        await dhScheduleService.TriggerScheduleAsync(requestBody).ConfigureAwait(false);
        return this.Ok();
    }
}
