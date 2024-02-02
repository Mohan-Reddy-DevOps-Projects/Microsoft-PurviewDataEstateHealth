#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Newtonsoft.Json.Linq;

/// <summary>
/// Health Reports controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/dataHealthControl")]
public class DHControlController(DHControlService dataHealthControlService) : DataPlaneController
{
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> ListById(string id)
    {
        var entity = await dataHealthControlService.GetControlByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateDHControlAsync(
        [FromBody] JObject payload)
    {
        var entity = DHControlBaseWrapper.Create(payload);
        await dataHealthControlService.CreateControlAsync(entity).ConfigureAwait(false);

        return this.Ok();
    }

    [HttpPost]
    [Route("runScheduleJob")]
    public async Task<ActionResult> CreateScheduleJob(
        [FromBody] DHRunScheduleJobRequest requestBody)
    {
        if (string.IsNullOrEmpty(requestBody.ControlId))
        {
            return this.BadRequest();
        }
        await dataHealthControlService.RunScheduleJob(requestBody.ControlId).ConfigureAwait(false);
        return this.Ok();
    }
}
