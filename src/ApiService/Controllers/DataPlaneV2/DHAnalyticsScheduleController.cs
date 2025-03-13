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
    IRequestHeaderContext requestHeaderContext,
    IAccountExposureControlConfigProvider exposureControl
   ) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> GetAnalyticsGlobalScheduleAsync()
    {
        var result = await analyticsScheduleService.GetAnalyticsGlobalScheduleAsync().ConfigureAwait(false);
        return this.Ok(result.JObject);
    }

    [HttpPost]
    [Route("trigger")]
    public async Task<ActionResult> TriggerAnalyticsScheduleAsync([FromBody] TriggerScheduleRequest requestPayload)
    {
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();
        if (!exposureControl.IsDataGovHealthTipsEnabled(accountId, string.Empty, tenantId))
        {
            logger.LogInformation($"Not allowed to trigger schedule. Account id: {accountId}. Tenant id: {tenantId}.");
            return this.Unauthorized();
        }
        using (logger.LogElapsed("Manually trigger schedule"))
        {
            var payload = new DHAnalyticsScheduleCallbackPayload
            {
                Operator = requestHeaderContext.ClientObjectId,
                TriggerType = DHAnalyticsScheduleCallbackTriggerType.Manually,
                ControlId = requestPayload.ControlId,
            };
            var scheduleRunId = await analyticsScheduleService.TriggerAnalyticsScheduleJobCallbackAsync(payload).ConfigureAwait(false);
            logger.LogInformation($"Manually trigger schedule successfully. ScheduleRunId: {scheduleRunId}. Operator: {requestHeaderContext.ClientObjectId}.");
            return this.Ok(new Dictionary<string, string>() { { "scheduleRunId", scheduleRunId } });
        }
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
        var result = await analyticsScheduleService.CreateOrUpdateAnalyticsGlobalScheduleAsync(entity).ConfigureAwait(false);
        return this.Ok(result.JObject);
    }
}

public record TriggerScheduleReq
{
    [JsonProperty("controlId")]
    public required string ControlId { get; set; }
}
