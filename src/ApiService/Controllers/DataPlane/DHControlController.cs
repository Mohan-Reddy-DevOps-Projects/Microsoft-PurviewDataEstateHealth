#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
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
[Route("/controls/v2")]
public class DHControlController(DHControlService dataHealthControlService, DHScheduleService dhScheduleService) : DataPlaneController
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

    [HttpPost]
    [Route("{controlId}/schedules")]
    public async Task<ActionResult> CreateSchedule(string controlId, [FromBody] JObject payload)
    {
        await dhScheduleService.ValidatePathnameScheduleId(controlId);
        var schedule = DHControlScheduleWrapper.Create(payload);
        schedule.ControlId = controlId;
        schedule.Validate();
        await dhScheduleService.CreateScheduleAsync(schedule).ConfigureAwait(false);
        return this.Created();
    }


    [HttpGet]
    [Route("{controlId}/schedules/{scheduleId}")]
    public async Task<ActionResult> GetSchedule(string controlId, string scheduleId)
    {
        var schedule = await dhScheduleService.ValidatePathnameScheduleId(controlId, scheduleId);
        return this.Ok(schedule);
    }

    [HttpPut]
    [Route("{controlId}/schedules/{scheduleId}")]
    public async Task<ActionResult> UpdateSchedule(string controlId, string scheduleId, [FromBody] JObject payload)
    {
        await dhScheduleService.ValidatePathnameScheduleId(controlId, scheduleId);
        var schedule = DHControlScheduleWrapper.Create(payload);
        schedule.ControlId = controlId;
        schedule.Id = scheduleId;
        schedule.Validate();
        await dhScheduleService.UpdateScheduleAsync(schedule).ConfigureAwait(false);
        return this.Ok(schedule);
    }

    [HttpDelete]
    [Route("{controlId}/schedules/{scheduleId}")]
    public async Task<ActionResult> DeleteSchedule(string controlId, string scheduleId)
    {
        var schedule = await dhScheduleService.ValidatePathnameScheduleId(controlId, scheduleId);
        await dhScheduleService.DeleteScheduleAsync(schedule).ConfigureAwait(false);
        return this.NoContent();
    }

    [HttpPost]
    [Route("{controlId}/schedules/{scheduleId}/trigger")]
    public async Task<ActionResult> CreateScheduleJob(string controlId, string scheduleId, [FromBody] DHRunScheduleJobRequest requestBody)
    {
        var schedule = await dhScheduleService.ValidatePathnameScheduleId(controlId, scheduleId);
        await dhScheduleService.TriggerScheduleAsync(schedule).ConfigureAwait(false);
        return this.Ok();
    }
}
