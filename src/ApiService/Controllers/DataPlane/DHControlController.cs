#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Newtonsoft.Json.Linq;

/// <summary>
/// Health Reports controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/controls/v2")]
public class DHControlController(DHControlService dataHealthControlService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListControlsAsync()
    {
        var batchResults = await dataHealthControlService.ListControlsAsync().ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateControlAsync(
    [FromBody] JObject payload)
    {
        var entity = DHControlBaseWrapper.Create(payload);
        var result = await dataHealthControlService.CreateControlAsync(entity).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{result.Id}"), result.JObject);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetControlById(string id)
    {
        var entity = await dataHealthControlService.GetControlByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    //[HttpPut]
    //[Route("{id}")]
    //public async Task<ActionResult> UpdateControlByIdAsync(string id, [FromBody] JObject payload)
    //{
    //    var entity = DHControlBaseWrapper.Create(payload);
    //    await dataHealthControlService.UpdateControlByIdAsync(id, entity).ConfigureAwait(false);
    //    return this.Ok();
    //}
}
