#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Interfaces;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/analytics/schedule")]
public class DHAnalyticsScheduleController(
    IDHAnalyticsScheduleService analyticsScheduleService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext
   ) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> GetAnalyticsGlobalScheduleAsync()
    {
        var result = await analyticsScheduleService.GetAnalyticsGlobalScheduleAsync().ConfigureAwait(false);
        return this.Ok(result.JObject);
    }


    [HttpPut]
    [Route("")]
    public async Task<ActionResult> CreateOrUpdateGlobalScheduleAsync(
    [FromBody] DHAnalyticsScheduleRequest payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        string jsonpayload = JsonConvert.SerializeObject(payload);
        JObject jsonpayloadobj = JObject.Parse(jsonpayload);
        var entity = new DHControlGlobalSchedulePayloadWrapper(jsonpayloadobj);
        logger.LogInformation($"Received create or update analytics schedule request: {jsonpayload}");
        var result = await analyticsScheduleService.CreateOrUpdateAnalyticsGlobalScheduleAsync(entity).ConfigureAwait(false);
        return this.Ok(result.JObject);
    }

    [HttpGet]
    [Route("getjoblogs")]
    public async Task<ActionResult> GetAnalyticsJobLogsGlobalScheduleAsync()
    {
        using (logger.LogElapsed("Manually trigger schedule"))
        {
            var accountId = requestHeaderContext.AccountObjectId.ToString();
            logger.LogInformation($"Getting analytics job logs for Account ID: {accountId}");
            var result = await analyticsScheduleService.GetDEHJobLogs(accountId).ConfigureAwait(false);
            return this.Ok(result);
        }
    }
}

public record TriggerScheduleReq
{
    [JsonProperty("controlId")]
    public required string ControlId { get; set; }
}
