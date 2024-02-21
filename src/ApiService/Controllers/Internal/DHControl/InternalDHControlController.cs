#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHControl;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

[ApiController]
[CertificateConfig(CertificateSet.DHControlSchedule)]
[Authorize(AuthenticationSchemes = "Certificate")]
[Route("/internal/control")]
public class InternalDHControlController(DHScheduleService dhScheduleService) : Controller
{
    [HttpPost]
    [Route("triggerScheduleJobCallback")]
    public async Task<ActionResult> TriggerScheduleJobRunCallback([FromBody] DHScheduleCallbackPayload requestBody)
    {
        if (requestBody == null)
        {
            return this.BadRequest();
        }
        if (String.IsNullOrEmpty(requestBody.ControlId))
        {
            return this.BadRequest($"ControlId is required.");
        }
        await dhScheduleService.TriggerScheduleAsync(requestBody).ConfigureAwait(false);
        return this.Ok();
    }
}
