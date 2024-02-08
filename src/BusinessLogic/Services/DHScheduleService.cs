namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using System.Collections.Generic;

    using System.Threading.Tasks;

    public class DHScheduleService
    {
        private readonly IRequestContextAccessor requestContextAccessor;
        private readonly DHControlRepository dhControlRepository;
        private readonly DHControlScheduleRepository dhControlScheduleRepository;
        private readonly IDataEstateHealthRequestLogger logger;
        private readonly ScheduleServiceClient scheduleServiceClient;
        private readonly DHScheduleConfiguration scheduleConfiguration;

        public DHScheduleService(
            IRequestContextAccessor requestContextAccessor,
            DHControlRepository dHControlRepository,
            DHControlScheduleRepository dhControlScheduleRepository,
            ScheduleServiceClientFactory scheduleServiceClientFactory,
            IOptions<DHScheduleConfiguration> scheduleConfiguration,
            IDataEstateHealthRequestLogger logger)
        {
            this.requestContextAccessor = requestContextAccessor;
            this.dhControlRepository = dHControlRepository;
            this.dhControlScheduleRepository = dhControlScheduleRepository;
            this.logger = logger;
            this.scheduleServiceClient = scheduleServiceClientFactory.GetClient();
            this.scheduleConfiguration = scheduleConfiguration.Value;
        }

        public async Task CreateScheduleAsync(DHControlScheduleWrapper schedule)
        {
            var schedulePayload = this.CreateScheduleRequestPayload(schedule);
            var response = await this.scheduleServiceClient.CreateSchedule(schedulePayload).ConfigureAwait(false);
            schedule.Id = response.ScheduleId;
            await this.dhControlScheduleRepository.AddAsync(schedule).ConfigureAwait(false);
        }

        public async Task UpdateScheduleAsync(DHControlScheduleWrapper schedule)
        {
            var schedulePayload = this.CreateScheduleRequestPayload(schedule);
            await this.scheduleServiceClient.UpdateSchedule(schedulePayload).ConfigureAwait(false);
            await this.dhControlScheduleRepository.UpdateAsync(schedule).ConfigureAwait(false);
        }

        public async Task DeleteScheduleAsync(DHControlScheduleWrapper schedule)
        {
            var schedulePayload = this.CreateScheduleRequestPayload(schedule);
            await this.scheduleServiceClient.DeleteSchedule(schedule.Id).ConfigureAwait(false);
            await this.dhControlScheduleRepository.DeleteAsync(schedule).ConfigureAwait(false);
        }

        public async Task TriggerScheduleAsync(string controlId)
        {
            // TOOD: query schedule and trigger it
            // await this.scheduleServiceClient.TriggerSchedule(scheduleId).ConfigureAwait(false);
            await Task.Delay(100);
        }

        private DHScheduleCreateRequestPayload CreateScheduleRequestPayload(DHControlScheduleWrapper schedule)
        {
            var payload = new DHScheduleCreateRequestPayload
            {
                ScheduleId = schedule.Id,
                CallbackRequest = new DHScheduleCreateRequestCallback
                {
                    Url = this.scheduleConfiguration.CallbackEndpoint + "/internal/control/triggerScheduleJobCallback",
                    Method = "POST",
                    Body = new DHScheduleCallbackPayload { ControlId = schedule.ControlId },
                    Headers = new Dictionary<string, string> { { "x-ms-client-tenant-id", this.requestContextAccessor.GetRequestContext().TenantId.ToString() } }
                }
            };
            payload.SetRecurrence(schedule);
            return payload;
        }
    }
}
