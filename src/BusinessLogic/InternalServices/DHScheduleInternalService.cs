#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices
{
    using Microsoft.Azure.Purview.DataEstateHealth.Common;
    using Microsoft.Azure.Purview.DataEstateHealth.Core;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHConfigurations;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHScheduleInternalService
    {
        private readonly DHControlScheduleRepository dhControlScheduleRepository;
        private readonly ScheduleServiceClient scheduleServiceClient;
        private readonly DHScheduleConfiguration scheduleConfiguration;
        private readonly IRequestHeaderContext requestHeaderContext;
        private readonly IDataEstateHealthRequestLogger logger;
        private readonly ICoreLayerFactory coreLayerFactory;

        public DHScheduleInternalService(
            IRequestHeaderContext requestHeaderContext,
            DHControlScheduleRepository dhControlScheduleRepository,
            ScheduleServiceClientFactory scheduleServiceClientFactory,
            IOptions<DHScheduleConfiguration> scheduleConfiguration,
            IDataEstateHealthRequestLogger logger,
            ICoreLayerFactory coreLayerFactory)
        {
            this.dhControlScheduleRepository = dhControlScheduleRepository;
            this.scheduleServiceClient = scheduleServiceClientFactory.GetClient();
            this.scheduleConfiguration = scheduleConfiguration.Value;
            this.requestHeaderContext = requestHeaderContext;
            this.logger = logger;
            this.coreLayerFactory = coreLayerFactory;
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> GetScheduleByIdAsync(string scheduleId)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(GetScheduleByIdAsync)}: Read for schedule with ID {scheduleId}"))
            {
                var schedule = await this.dhControlScheduleRepository.GetByIdAsync(scheduleId).ConfigureAwait(false);

                if (schedule == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), scheduleId));
                }

                return schedule;
            }
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> CreateScheduleAsync(DHControlScheduleStoragePayloadWrapper schedule, string? controlId = null)
        {
            schedule.Host = DHControlScheduleHost.AzureStack;
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateScheduleAsync)}. Tenant Id: {this.requestHeaderContext.TenantId}. Account Id: {this.requestHeaderContext.AccountObjectId}."))
            {
                await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreateDHWorkerServiceTriggerComponent(this.requestHeaderContext.TenantId, this.requestHeaderContext.AccountObjectId)
                    .UpsertDEHScheduleJob(schedule.Properties).ConfigureAwait(false);

                var scheduleId = GenerateScheduleId(Guid.NewGuid());
                this.logger.LogInformation($"DEH Schedule job is created in worker service. Schedule id: {scheduleId}");

                schedule.OnCreate(this.requestHeaderContext.AccountObjectId.ToString(), scheduleId);

                await this.dhControlScheduleRepository.AddAsync(schedule).ConfigureAwait(false);

                return schedule;
            }
        }

        public async Task<DHControlScheduleStoragePayloadWrapper> UpdateScheduleAsync(DHControlScheduleStoragePayloadWrapper schedule, string? controlId = null)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateScheduleAsync)}: Update for schedule with ID {schedule.Id}.  Tenant Id: {this.requestHeaderContext.TenantId}. Account Id: {this.requestHeaderContext.AccountObjectId}."))
            {
                var existEntity = await this.dhControlScheduleRepository.GetByIdAsync(schedule.Id).ConfigureAwait(false);

                if (existEntity == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), schedule.Id));
                }

                schedule.Host = existEntity.Host;

                if (schedule.Host == DHControlScheduleHost.DGScheduleService)
                {
                    var schedulePayload = this.CreateScheduleRequestPayload(schedule, controlId);
                    await this.scheduleServiceClient.UpdateSchedule(schedulePayload).ConfigureAwait(false);
                    this.logger.LogInformation($"DEH Schedule job is updated in schedule service. Schedule id: {schedule.Id}");
                }
                else if (schedule.Host == DHControlScheduleHost.AzureStack)
                {
                    await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                        .CreateDHWorkerServiceTriggerComponent(this.requestHeaderContext.TenantId, this.requestHeaderContext.AccountObjectId)
                        .UpsertDEHScheduleJob(schedule.Properties).ConfigureAwait(false);
                    this.logger.LogInformation($"DEH Schedule job is updated in worker service. Schedule id: {schedule.Id}");
                }
                else
                {
                    throw new Exception($"Invalid schedule host: {schedule.Host}.");
                }

                schedule.OnUpdate(existEntity, this.requestHeaderContext.AccountObjectId.ToString());

                await this.dhControlScheduleRepository.UpdateAsync(schedule).ConfigureAwait(false);

                return schedule;
            }
        }

        public async Task DeleteScheduleAsync(string scheduleId)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(DeleteScheduleAsync)}: Delete for schedule with ID {scheduleId}.  Tenant Id: {this.requestHeaderContext.TenantId}. Account Id: {this.requestHeaderContext.AccountObjectId}."))
            {
                var schedule = await this.dhControlScheduleRepository.GetByIdAsync(scheduleId).ConfigureAwait(false);
                if (schedule == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), scheduleId));
                }
                if (schedule.Host == DHControlScheduleHost.DGScheduleService)
                {
                    await this.scheduleServiceClient.DeleteSchedule(scheduleId).ConfigureAwait(false);
                }
                else if (schedule.Host == DHControlScheduleHost.AzureStack)
                {
                    await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreateDHWorkerServiceTriggerComponent(this.requestHeaderContext.TenantId, this.requestHeaderContext.AccountObjectId)
                    .DeleteDEHScheduleJob().ConfigureAwait(false);
                }
                else
                {
                    throw new Exception($"Invalid schedule host: {schedule.Host}.");
                }

                await this.dhControlScheduleRepository.DeleteAsync(scheduleId).ConfigureAwait(false);
            }
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
                        Operator = DHScheduleCallbackPayload.DGScheduleServiceOperatorName,
                        TriggerType = DHScheduleCallbackTriggerType.Schedule,
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
        public async Task DeprovisionForSchedulesAsync()
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForSchedulesAsync)}: Deprovision all schedules"))
            {
                var allSchedules = await this.dhControlScheduleRepository.GetAllAsync().ConfigureAwait(false);

                this.logger.LogInformation($"Found {allSchedules.Count()} schedules to deprovision. Schedule IDs: {String.Join(", ", allSchedules.Select(s => s.Id) ?? [])}");

                await Task.WhenAll(allSchedules.Select(async schedule =>
                {
                    try
                    {
                        await this.DeleteScheduleAsync(schedule.Id).ConfigureAwait(false);

                        this.logger.LogInformation($"Successfully deprovision schedule with ID {schedule.Id}.");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"Failed to deprovision schedule with ID {schedule.Id}.", ex);
                    }
                })).ConfigureAwait(false);
            }
        }

        // Schedule id generate logic is copied from DG schedule service.
        private static string GenerateScheduleId(Guid scheduleId)
        {
            var id = Convert.ToBase64String(scheduleId.ToByteArray())
                .Replace("=", string.Empty, StringComparison.CurrentCultureIgnoreCase)
                .Replace("+", "-", StringComparison.CurrentCultureIgnoreCase)
                .Replace("/", "_", StringComparison.CurrentCultureIgnoreCase);
            return $"DGS{DateTime.Now:yyMMddHHmmss}{id}";
        }
    }
}
