#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices
{
    using Microsoft.Azure.Purview.DataEstateHealth.Common;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    public class DHScheduleInternalService
    {
        private readonly DHControlScheduleRepository dhControlScheduleRepository;
        private readonly ScheduleServiceClient scheduleServiceClient;
        private readonly DHScheduleConfiguration scheduleConfiguration;
        private readonly IRequestHeaderContext requestHeaderContext;

        public DHScheduleInternalService(
            IRequestHeaderContext requestHeaderContext,
            DHControlScheduleRepository dhControlScheduleRepository,
            ScheduleServiceClientFactory scheduleServiceClientFactory,
            IOptions<DHScheduleConfiguration> scheduleConfiguration)
        {
            this.dhControlScheduleRepository = dhControlScheduleRepository;
            this.scheduleServiceClient = scheduleServiceClientFactory.GetClient();
            this.scheduleConfiguration = scheduleConfiguration.Value;
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

        public async Task<DHControlScheduleStoragePayloadWrapper> CreateScheduleAsync(DHControlScheduleStoragePayloadWrapper schedule, string? controlId = null)
        {
            var schedulePayload = this.CreateScheduleRequestPayload(schedule, controlId);
            var response = await this.scheduleServiceClient.CreateSchedule(schedulePayload).ConfigureAwait(false);

            schedule.OnCreate(this.requestHeaderContext.ClientObjectId, response.ScheduleId);

            await this.dhControlScheduleRepository.AddAsync(schedule).ConfigureAwait(false);

            return schedule;
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> UpdateScheduleAsync(DHControlScheduleStoragePayloadWrapper schedule, string? controlId = null)
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

        private DHScheduleCreateRequestPayload CreateScheduleRequestPayload(DHControlScheduleStoragePayloadWrapper schedule, string? controlId)
        {
            var tenantId = this.requestHeaderContext.TenantId.ToString();
            var accountId = this.requestHeaderContext.AccountObjectId.ToString();
            var payload = new DHScheduleCreateRequestPayload
            {
                ScheduleId = schedule.Id,
                CallbackRequest = new DHScheduleCreateRequestCallback
                {
                    Url = $"{this.scheduleConfiguration.CallbackEndpoint}/internal/control/triggerScheduleJobCallback?api-version={ServiceVersion.LabelV2}",
                    Method = "POST",
                    Body = new DHScheduleCallbackPayload
                    {
                        ControlId = controlId,
                    },
                    Headers = new Dictionary<string, string>
                    {
                        ["x-ms-client-tenant-id"] = tenantId,
                        ["x-ms-account-id"] = accountId
                    }
                },
                State = schedule.Properties.Status,
            };
            payload.SetRecurrence(schedule.Properties);
            return payload;
        }
    }
}
