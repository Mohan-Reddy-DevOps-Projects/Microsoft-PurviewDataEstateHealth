namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHControlService(
        DHControlRepository dHControlRepository,
        DHScheduleInternalService scheduleService,
        DHAssessmentService assessmentService,
        IRequestHeaderContext requestHeaderContext)
    {
        public async Task<IBatchResults<DHControlBaseWrapper>> ListControlsAsync()
        {
            var entities = await dHControlRepository.GetAllAsync().ConfigureAwait(false);

            var results = await Task.WhenAll(entities.Select(e => this.ReadEntityScheduleAsync(e)));

            return new BatchResults<DHControlBaseWrapper>(results, results.Length);
        }

        public async Task<DHControlBaseWrapper> GetControlByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var entity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (entity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Control.ToString(), id));
            }

            var result = await this.ReadEntityScheduleAsync(entity);

            return result;
        }

        public async Task<DHControlBaseWrapper> CreateControlAsync(DHControlBaseWrapper entity, bool withNewAssessment = false, bool isSystem = false)
        {
            ArgumentNullException.ThrowIfNull(entity);

            entity.Validate();

            if (!isSystem)
            {
                entity.NormalizeInput();
            }

            if (withNewAssessment)
            {
                switch (entity.Type)
                {
                    case DHControlBaseWrapperDerivedTypes.Node:
                        var assessment = await assessmentService.CreateEmptyAssessmentAsync(entity.Name).ConfigureAwait(false);
                        var nodeEntity = (DHControlNodeWrapper)entity;
                        nodeEntity.AssessmentId = assessment.Id;
                        break;
                    default:
                        throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageAddAssessmentOnlyOnNode, entity.Type));
                }
            }

            entity.OnCreate(requestHeaderContext.ClientObjectId);

            var result = await this.CreateEntityScheduleAsync(entity);

            await dHControlRepository.AddAsync(result).ConfigureAwait(false);

            return entity;
        }

        public async Task<DHControlBaseWrapper> UpdateControlByIdAsync(string id, DHControlBaseWrapper entity)
        {
            ArgumentNullException.ThrowIfNull(id);

            ArgumentNullException.ThrowIfNull(entity);

            if (!string.IsNullOrEmpty(entity.Id) && !string.Equals(id, entity.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageUpdateEntityIdNotMatch, EntityCategory.StatusPalette.ToString(), entity.Id, id));
            }

            entity.Validate();
            entity.NormalizeInput();

            var existEntity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existEntity == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.StatusPalette.ToString(), id));
            }

            entity.OnUpdate(existEntity, requestHeaderContext.ClientObjectId);

            var result = await this.UpdateEntityScheduleAsync(existEntity, entity);

            await dHControlRepository.UpdateAsync(result).ConfigureAwait(false);

            return entity;
        }

        public async Task DeleteControlByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            var existEntity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

            if (existEntity == null)
            {
                // Log

                return;
            }

            await this.DeleteEntityScheduleAsync(existEntity);

            await dHControlRepository.DeleteAsync(id).ConfigureAwait(false);
        }

        private async Task<DHControlBaseWrapper> ReadEntityScheduleAsync(DHControlBaseWrapper entity)
        {
            if (entity.Type == DHControlBaseWrapperDerivedTypes.Node)
            {
                var node = (DHControlNodeWrapper)entity;

                var scheduleId = node.ScheduleId;

                if (!string.IsNullOrEmpty(scheduleId))
                {
                    var scheduleEntity = await scheduleService.GetScheduleByIdAsync(scheduleId).ConfigureAwait(false);
                    node.Schedule = scheduleEntity.Properties;
                }

                node.ScheduleId = null;

                return node;
            }
            return entity;
        }

        private async Task<DHControlBaseWrapper> CreateEntityScheduleAsync(DHControlBaseWrapper entity)
        {
            if (entity.Type == DHControlBaseWrapperDerivedTypes.Node)
            {
                var node = (DHControlNodeWrapper)entity;

                var schedule = node.Schedule;

                if (schedule != null)
                {
                    var scheduleWrapper = new DHControlScheduleStoragePayloadWrapper([]);
                    scheduleWrapper.Properties = schedule;
                    scheduleWrapper.Type = DHControlScheduleType.ControlNode;

                    var scheduleEntity = await scheduleService.CreateScheduleAsync(scheduleWrapper, entity.Id).ConfigureAwait(false);
                    node.ScheduleId = scheduleEntity.Id;
                }

                node.Schedule = null;

                return node;
            }
            return entity;
        }

        private async Task<DHControlBaseWrapper> UpdateEntityScheduleAsync(DHControlBaseWrapper existEntity, DHControlBaseWrapper newEntity)
        {
            if (existEntity.Type == DHControlBaseWrapperDerivedTypes.Node)
            {
                var existNode = (DHControlNodeWrapper)existEntity;

                var newNode = (DHControlNodeWrapper)newEntity;

                var schedule = newNode.Schedule;

                if (schedule != null)
                {
                    var scheduleWrapper = new DHControlScheduleStoragePayloadWrapper([]);
                    scheduleWrapper.Properties = schedule;
                    scheduleWrapper.Type = DHControlScheduleType.ControlNode;

                    if (existNode.ScheduleId != null)
                    {
                        scheduleWrapper.Id = existNode.ScheduleId;
                        await scheduleService.UpdateScheduleAsync(scheduleWrapper, existEntity.Id).ConfigureAwait(false);
                    }
                    else
                    {
                        var scheduleEntity = await scheduleService.CreateScheduleAsync(scheduleWrapper, existEntity.Id).ConfigureAwait(false);
                        newNode.ScheduleId = scheduleEntity.Id;
                    }
                }

                newNode.Schedule = null;

                return newNode;
            }
            return newEntity;
        }

        private async Task DeleteEntityScheduleAsync(DHControlBaseWrapper entity)
        {
            if (entity.Type == DHControlBaseWrapperDerivedTypes.Node)
            {
                var node = (DHControlNodeWrapper)entity;

                var scheduleId = node.ScheduleId;

                if (!string.IsNullOrEmpty(scheduleId))
                {
                    await scheduleService.DeleteScheduleAsync(scheduleId).ConfigureAwait(false);
                }
            }
        }
    }
}
