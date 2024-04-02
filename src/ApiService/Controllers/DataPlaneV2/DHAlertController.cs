namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Alert;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/alerts")]
public class DHAlertController(DHAlertService alertService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListAlertsAsync()
    {
        var batchResults = await alertService.ListAlertsAsync().ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateAlertAsync([FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHAlertWrapper.Create(payload);
        var result = await alertService.CreateAlertAsync(entity).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{result.Id}"), result.JObject);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetAlertById(string id)
    {
        var entity = await alertService.GetAlertByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> UpdateAlertByIdAsync(string id, [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHAlertWrapper.Create(payload!);
        var result = await alertService.UpdateAlertByIdAsync(id, entity).ConfigureAwait(false);
        return this.Ok(result.JObject);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteAlertByIdAsync(string id)
    {
        await alertService.DeleteAlertByIdAsync(id).ConfigureAwait(false);
        return this.NoContent();
    }
}
