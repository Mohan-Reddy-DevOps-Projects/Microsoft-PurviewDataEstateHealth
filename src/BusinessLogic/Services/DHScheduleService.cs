#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DHScheduleService(
    DHScheduleInternalService scheduleService,
    DHControlScheduleRepository dhControlScheduleRepository,
    IDataQualityExecutionService dataQualityExecutionService)
{
    public async Task TriggerScheduleAsync(DHScheduleCallbackPayload payload)
    {
        await dataQualityExecutionService.SubmitDQJob(payload.AccountId, payload.ControlId, Guid.NewGuid().ToString());

        // TOOD: query schedule and trigger it
        // await this.scheduleServiceClient.TriggerSchedule(scheduleId).ConfigureAwait(false);
        await Task.Delay(100);
    }

    public async Task<DHControlGlobalSchedulePayloadWrapper> CreateOrUpdateGlobalScheduleAsync(DHControlGlobalSchedulePayloadWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.Validate();
        entity.NormalizeInput();

        var existingGlobalSchedule = await this.GetGlobalScheduleInternalAsync().ConfigureAwait(false);

        var scheduleStoragePayload = DHControlScheduleStoragePayloadWrapper.Create([]);
        scheduleStoragePayload.Properties = entity;
        scheduleStoragePayload.Type = DHControlScheduleType.ControlGlobal;

        if (existingGlobalSchedule == null)
        {
            // Create Global Schedule
            var result = await scheduleService.CreateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
            entity.AuditLogs = result.AuditLogs;
            return entity;
        }
        else
        {
            // Update Global Schedule
            scheduleStoragePayload.Id = existingGlobalSchedule.Id;
            var result = await scheduleService.UpdateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
            entity.AuditLogs = result.AuditLogs;
            return entity;
        }
    }

    public async Task<DHControlGlobalSchedulePayloadWrapper> GetGlobalScheduleAsync()
    {
        var globalSchedule = await this.GetGlobalScheduleInternalAsync().ConfigureAwait(false);

        if (globalSchedule == null)
        {
            throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), "Global"));
        }

        var response = new DHControlGlobalSchedulePayloadWrapper(globalSchedule.Properties.JObject);
        response.AuditLogs = globalSchedule.AuditLogs;

        return response;
    }

    private async Task<DHControlScheduleStoragePayloadWrapper?> GetGlobalScheduleInternalAsync()
    {
        var globalScheduleQueryResult = await dhControlScheduleRepository.QueryScheduleAsync(DHControlScheduleType.ControlGlobal).ConfigureAwait(false);

        return globalScheduleQueryResult.FirstOrDefault();
    }
}
