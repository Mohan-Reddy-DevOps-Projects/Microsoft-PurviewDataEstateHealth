namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using System;
using System.Threading.Tasks;

public class DHScheduleService(
    //DHScheduleInternalService scheduleService,
    IDataQualityExecutionService dataQualityExecutionService)
{
    public async Task TriggerScheduleAsync(DHScheduleCallbackPayload payload)
    {
        await dataQualityExecutionService.SubmitDQJob(payload.AccountId, payload.ControlId, Guid.NewGuid().ToString());

        // TOOD: query schedule and trigger it
        // await this.scheduleServiceClient.TriggerSchedule(scheduleId).ConfigureAwait(false);
        await Task.Delay(100);
    }
}
