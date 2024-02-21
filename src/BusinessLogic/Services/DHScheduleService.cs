namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
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
        private readonly IDataQualityExecutionService dataQualityExecutionService;
        private readonly IRequestHeaderContext requestHeaderContext;

        public DHScheduleService(
            IRequestContextAccessor requestContextAccessor,
            IRequestHeaderContext requestHeaderContext,
            DHControlRepository dHControlRepository,
            DHControlScheduleRepository dhControlScheduleRepository,
            ScheduleServiceClientFactory scheduleServiceClientFactory,
            IOptions<DHScheduleConfiguration> scheduleConfiguration,
            IDataQualityExecutionService dataQualityExecutionService,
            IDataEstateHealthRequestLogger logger)
        {
            this.requestContextAccessor = requestContextAccessor;
            this.dhControlRepository = dHControlRepository;
            this.dhControlScheduleRepository = dhControlScheduleRepository;
            this.logger = logger;
            this.scheduleServiceClient = scheduleServiceClientFactory.GetClient();
            this.scheduleConfiguration = scheduleConfiguration.Value;
            this.dataQualityExecutionService = dataQualityExecutionService;
            this.requestHeaderContext = requestHeaderContext;
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> GetScheduleByIdAsync(string scheduleId)
        {
            var schedule = await this.dhControlScheduleRepository.GetByIdAsync(scheduleId).ConfigureAwait(false);

            if (schedule == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), scheduleId));
            }

            return schedule;
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> CreateScheduleAsync(DHControlScheduleStoragePayloadWrapper schedule, string controlId)
        {
            var schedulePayload = this.CreateScheduleRequestPayload(schedule, controlId);
            var response = await this.scheduleServiceClient.CreateSchedule(schedulePayload).ConfigureAwait(false);

            schedule.OnCreate(this.requestHeaderContext.ClientObjectId);
            schedule.Id = response.ScheduleId;

            await this.dhControlScheduleRepository.AddAsync(schedule).ConfigureAwait(false);

            return schedule;
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> UpdateScheduleAsync(DHControlScheduleStoragePayloadWrapper schedule, string controlId)
        {
            var existEntity = await this.dhControlScheduleRepository.GetByIdAsync(schedule.Id).ConfigureAwait(false);

            if (existEntity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), schedule.Id));
            }

            var schedulePayload = this.CreateScheduleRequestPayload(schedule, controlId);
            await this.scheduleServiceClient.UpdateSchedule(schedulePayload).ConfigureAwait(false);

            schedule.OnUpdate(existEntity, this.requestHeaderContext.ClientObjectId);

            await this.dhControlScheduleRepository.UpdateAsync(schedule).ConfigureAwait(false);

            return schedule;
        }

        public async Task DeleteScheduleAsync(string scheduleId)
        {
            await this.scheduleServiceClient.DeleteSchedule(scheduleId).ConfigureAwait(false);
            await this.dhControlScheduleRepository.DeleteAsync(scheduleId).ConfigureAwait(false);
        }

        public async Task TriggerScheduleAsync(DHScheduleCallbackPayload payload)
        {
            await this.dataQualityExecutionService.SubmitDQJob(payload.AccountId);

            // TOOD: query schedule and trigger it
            // await this.scheduleServiceClient.TriggerSchedule(scheduleId).ConfigureAwait(false);
            await Task.Delay(100);
        }

        private DHScheduleCreateRequestPayload CreateScheduleRequestPayload(DHControlScheduleStoragePayloadWrapper schedule, string controlId)
        {
            var payload = new DHScheduleCreateRequestPayload
            {
                ScheduleId = schedule.Id,
                CallbackRequest = new DHScheduleCreateRequestCallback
                {
                    Url = this.scheduleConfiguration.CallbackEndpoint + "/internal/control/triggerScheduleJobCallback",
                    Method = "POST",
                    Body = new DHScheduleCallbackPayload
                    {
                        ControlId = controlId,
                        TenantId = this.requestContextAccessor.GetRequestContext().TenantId.ToString(),
                        AccountId = this.requestContextAccessor.GetRequestContext().AccountObjectId.ToString(),
                    },
                    Headers = new Dictionary<string, string> { { "x-ms-client-tenant-id", this.requestContextAccessor.GetRequestContext().TenantId.ToString() } }
                }
            };
            payload.SetRecurrence(schedule.Properties);
            return payload;
        }
    }
}
