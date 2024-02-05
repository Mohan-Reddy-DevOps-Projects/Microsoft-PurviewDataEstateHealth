namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using System;
    using System.Threading.Tasks;

    public class DHScheduleService
    {
        DHControlRepository dhControlRepository;
        DHControlScheduleRepository dhControlScheduleRepository;
        IDataEstateHealthRequestLogger logger;
        ScheduleServiceClient scheduleServiceClient;

        public DHScheduleService(DHControlRepository dHControlRepository,
            DHControlScheduleRepository dhControlScheduleRepository,
            ScheduleServiceClientFactory scheduleServiceClientFactory,
            IDataEstateHealthRequestLogger logger)
        {
            this.dhControlRepository = dHControlRepository;
            this.dhControlScheduleRepository = dhControlScheduleRepository;
            this.logger = logger;
            this.scheduleServiceClient = scheduleServiceClientFactory.GetClient();
        }

        public async Task CreateScheduleAsync(DHControlScheduleWrapper entity)
        {
            await this.ValidateControlId(entity.ControlId);
            await this.dhControlScheduleRepository.AddAsync(entity).ConfigureAwait(false);
        }

        public async Task UpdateScheduleAsync(DHControlScheduleWrapper entity)
        {
            await this.ValidateControlId(entity.ControlId);
            await this.dhControlScheduleRepository.UpdateAsync(entity).ConfigureAwait(false);
        }

        public async Task DeleteScheduleAsync(DHControlScheduleWrapper schedule)
        {
            await this.dhControlScheduleRepository.DeleteAsync(schedule).ConfigureAwait(false);
        }

        public async Task TriggerScheduleAsync(DHControlScheduleWrapper schedule)
        {
            this.logger.LogInformation($"Schedule job triggered in control {schedule.Id}");
            await this.scheduleServiceClient.TriggerSchedule(Guid.Parse(schedule.Id)).ConfigureAwait(false);
        }

        public async Task<DHControlBaseWrapper> ValidatePathnameScheduleId(string controlId)
        {
            return await this.ValidateControlId(controlId);
        }

        public async Task<DHControlScheduleWrapper> ValidatePathnameScheduleId(string controlId, string scheduleId)
        {
            var control = await this.ValidatePathnameScheduleId(controlId);
            var schedule = await this.ValidateScheduleId(scheduleId);
            if (schedule.ControlId != control.Id)
            {
                throw new ControlNotMatchedException($"Dismatched control found in schedule {scheduleId}");
            }
            return schedule;
        }

        private async Task<DHControlBaseWrapper> ValidateControlId(string id)
        {
            var control = await this.dhControlRepository.GetByIdAsync(id).ConfigureAwait(false);
            if (null == control)
            {
                throw new ControlNotFoundException($"Control not found: {id}");
            }
            return control;
        }

        private async Task<DHControlScheduleWrapper> ValidateScheduleId(string id)
        {
            var schedule = await this.dhControlScheduleRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (null == schedule)
            {
                throw new ControlNotFoundException($"Schedule not found: {id}");
            }
            return schedule;
        }
    }
}
