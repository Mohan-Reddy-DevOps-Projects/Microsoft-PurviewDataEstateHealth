#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;

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
    public async Task<ActionResult> ListById(Guid id)
    {
        var entity = await dataHealthControlService.GetControlByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateDHSimpleRuleAsync(
        [FromBody] DHControlNode entity)
    {
        await dataHealthControlService.CreateControlAsync(entity).ConfigureAwait(false);
        return this.Ok();
    }
}
