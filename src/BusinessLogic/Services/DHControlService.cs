namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHControlService(
        DHControlRepository dHControlRepository,
        DHScheduleInternalService scheduleService,
        DHStatusPaletteInternalService statusPaletteInternalService,
        DHAssessmentService assessmentService,
        IRequestHeaderContext requestHeaderContext,
        IDataEstateHealthRequestLogger logger)
    {
        public async Task<IBatchResults<DHControlBaseWrapper>> ListControlsAsync()
        {
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ListControlsAsync)}"))
            {
                var entities = await dHControlRepository.GetAllAsync().ConfigureAwait(false);

                logger.LogInformation($"Found {entities.Count()} controls. Start reading schedule information.");

                var results = await Task.WhenAll(entities.Select(e => this.ReadEntityScheduleAsync(e)));

                return new BatchResults<DHControlBaseWrapper>(results, results.Length);
            }
        }

        public async Task<DHControlBaseWrapper> GetControlByIdAsync(string id)
        {
            ArgumentNullException.ThrowIfNull(id);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(GetControlByIdAsync)}: Read for control with ID {id}"))
            {
                var entity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

                if (entity == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Control.ToString(), id));
                }

                var result = await this.ReadEntityScheduleAsync(entity);

                return result;
            }
        }

        public async Task<DHControlBaseWrapper> CreateControlAsync(DHControlBaseWrapper entity, bool withNewAssessment = false, bool isSystem = false)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateControlAsync)}: Create control. Created by system: {isSystem}"))
            {
                entity.Validate();

                if (!isSystem)
                {
                    entity.NormalizeInput();

                    if (entity.Status == DHControlStatus.InDevelopment)
                    {
                        throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageInDevelopStatus));
                    }
                }

                await this.ValidateStatusPaletteConfig(entity).ConfigureAwait(false);

                logger.LogInformation($"Creating with control type {entity.Type}.");

                switch (entity.Type)
                {
                    case DHControlBaseWrapperDerivedTypes.Node:
                        var nodeEntity = (DHControlNodeWrapper)entity;
                        if (withNewAssessment)
                        {
                            logger.LogInformation($"Creating new assessment for control.");

                            var assessment = await assessmentService.CreateEmptyAssessmentAsync(entity.Name).ConfigureAwait(false);
                            nodeEntity.AssessmentId = assessment.Id;

                            logger.LogInformation($"Created new assessment with ID {assessment.Id} for control.");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(nodeEntity.AssessmentId))
                            {
                                throw new EntityValidationException(String.Format(
                                    CultureInfo.InvariantCulture,
                                    StringResources.ErrorMessagePropertyRequired,
                                    DHControlNodeWrapper.keyAssessmentId));
                            }
                            var assessment = await assessmentService.GetAssessmentByIdAsync(nodeEntity.AssessmentId).ConfigureAwait(false);
                            if (assessment == null)
                            {
                                throw new EntityValidationException(String.Format(
                                    CultureInfo.InvariantCulture,
                                    StringResources.ErrorMessageReferenceNotFound,
                                    EntityCategory.Assessment.ToString(),
                                    nodeEntity.AssessmentId));
                            }
                        }

                        if (nodeEntity.GroupId != null)
                        {
                            logger.LogInformation($"Validating group ID {nodeEntity.GroupId} for control.");

                            var group = await dHControlRepository.GetByIdAsync(nodeEntity.GroupId).ConfigureAwait(false);
                            if (group == null)
                            {
                                throw new EntityValidationException(String.Format(
                                    CultureInfo.InvariantCulture,
                                    StringResources.ErrorMessageReferenceNotFound,
                                    EntityCategory.Control.ToString(),
                                    nodeEntity.GroupId));
                            }
                            if (group.Type != DHControlBaseWrapperDerivedTypes.Group)
                            {
                                throw new EntityValidationException(String.Format(
                                    CultureInfo.InvariantCulture,
                                    StringResources.ErrorMessageReferenceNotMatch,
                                    EntityCategory.Control.ToString(),
                                    DHControlNodeWrapper.keyGroupId,
                                    nodeEntity.GroupId,
                                    nodeEntity.Type,
                                    DHControlBaseWrapperDerivedTypes.Group));
                            }
                        }

                        break;
                    default:
                        if (withNewAssessment)
                        {
                            throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageAddAssessmentOnlyOnNode, entity.Type));
                        }
                        break;
                }

                entity.OnCreate(requestHeaderContext.ClientObjectId);

                var result = await this.CreateEntityScheduleAsync(entity);

                await dHControlRepository.AddAsync(result).ConfigureAwait(false);

                return entity;
            }
        }

        public async Task<DHControlBaseWrapper> UpdateControlByIdAsync(string id, DHControlBaseWrapper entity, bool isSystem = false)
        {
            ArgumentNullException.ThrowIfNull(id);

            ArgumentNullException.ThrowIfNull(entity);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateControlByIdAsync)}: Update control with ID {id}. Updated by system: {isSystem}"))
            {
                if (!string.IsNullOrEmpty(entity.Id) && !string.Equals(id, entity.Id, StringComparison.OrdinalIgnoreCase))
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageUpdateEntityIdNotMatch, EntityCategory.Control.ToString(), entity.Id, id));
                }

                entity.Validate();

                if (!isSystem)
                {
                    entity.NormalizeInput();
                }

                await this.ValidateStatusPaletteConfig(entity).ConfigureAwait(false);

                var existEntity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

                if (existEntity == null)
                {
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Control.ToString(), id));
                }

                if (existEntity.Type != entity.Type)
                {
                    throw new EntityValidationException(String.Format(
                        CultureInfo.InvariantCulture,
                        StringResources.ErrorMessagePropertyCannotBeChanged,
                        DHControlBaseWrapper.keyType,
                        existEntity.Type,
                        entity.Type));
                }

                if (!isSystem && existEntity.Status == DHControlStatus.InDevelopment)
                {
                    throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageInDevelopStatus));
                }

                logger.LogInformation($"Updating with control type {existEntity.Type}.");

                if (existEntity.Type == DHControlBaseWrapperDerivedTypes.Node)
                {
                    var newNodeEntity = (DHControlNodeWrapper)entity;
                    var existNodeEntity = (DHControlNodeWrapper)existEntity;

                    if (!isSystem &&
                        !string.IsNullOrEmpty(existNodeEntity.GroupId) &&
                        !string.Equals(existNodeEntity.GroupId, newNodeEntity.GroupId, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new EntityValidationException(String.Format(
                            CultureInfo.InvariantCulture,
                            StringResources.ErrorMessagePropertyCannotBeChanged,
                            DHControlNodeWrapper.keyGroupId,
                            existNodeEntity.GroupId,
                            newNodeEntity.GroupId));
                    }

                    if (!string.IsNullOrEmpty(existNodeEntity.AssessmentId) &&
                        !string.Equals(existNodeEntity.AssessmentId, newNodeEntity.AssessmentId, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new EntityValidationException(String.Format(
                            CultureInfo.InvariantCulture,
                            StringResources.ErrorMessagePropertyCannotBeChanged,
                            DHControlNodeWrapper.keyAssessmentId,
                            existNodeEntity.AssessmentId,
                            newNodeEntity.AssessmentId));
                    }
                }

                entity.OnUpdate(existEntity, requestHeaderContext.ClientObjectId);

                var result = await this.UpdateEntityScheduleAsync(existEntity, entity);

                await dHControlRepository.UpdateAsync(result).ConfigureAwait(false);

                return entity;
            }
        }

        public async Task DeleteControlByIdAsync(string id, bool deleteAssessment = false)
        {
            ArgumentNullException.ThrowIfNull(id);

            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeleteControlByIdAsync)}: Delete control with ID {id}."))
            {
                var existEntity = await dHControlRepository.GetByIdAsync(id).ConfigureAwait(false);

                if (existEntity == null)
                {
                    logger.LogWarning($"Control with ID {id} not found. No action taken.");

                    return;
                }

                var referencedControls = await dHControlRepository.QueryControlNodesAsync(new ControlNodeFilters() { ParentControlIds = [id] }).ConfigureAwait(false);

                if (referencedControls.Any())
                {
                    throw new EntityReferencedException(String.Format(
                        CultureInfo.InvariantCulture,
                        StringResources.ErrorMessageDeleteFailureEntityReferenced,
                        EntityCategory.Control.ToString(),
                        id,
                        EntityCategory.Control,
                        String.Join(", ", referencedControls.Select(x => $"\"{x.Id}\""))));
                }

                await this.DeleteEntityScheduleAsync(existEntity);

                await dHControlRepository.DeleteAsync(id).ConfigureAwait(false);

                if (deleteAssessment)
                {
                    if (existEntity.Type == DHControlBaseWrapperDerivedTypes.Node)
                    {
                        var nodeEntity = (DHControlNodeWrapper)existEntity;
                        var assessmentId = nodeEntity.AssessmentId;
                        if (!string.IsNullOrEmpty(assessmentId))
                        {
                            await assessmentService.DeleteAssessmentByIdAsync(assessmentId).ConfigureAwait(false);
                        }
                    }
                }
            }
        }

        public async Task DeprovisionForControlsAsync()
        {
            using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionForControlsAsync)}: Deprovision for assessments"))
            {
                await dHControlRepository.DeprovisionAsync().ConfigureAwait(false);
            }
        }

        private async Task ValidateStatusPaletteConfig(DHControlBaseWrapper wrapper)
        {
            if (wrapper.StatusPaletteConfig != null)
            {
                logger.LogInformation($"StatusPaletteConfig is not null in wrapper. Start to validate the StatusPaletteConfig.");

                var statusPaletteIds = new HashSet<string>([wrapper.StatusPaletteConfig.FallbackStatusPaletteId], StringComparer.OrdinalIgnoreCase);
                foreach (var statusPaletteConfig in wrapper.StatusPaletteConfig.StatusPaletteRules ?? [])
                {
                    statusPaletteIds.Add(statusPaletteConfig.StatusPaletteId);
                }

                var resultStatusPalettes = await statusPaletteInternalService.QueryStatusPalettesAsync(new StatusPaletteFilters() { ids = statusPaletteIds.ToList() }).ConfigureAwait(false);

                if (resultStatusPalettes.Count() != statusPaletteIds.Count)
                {
                    throw new EntityValidationException(String.Format(
                        CultureInfo.InvariantCulture,
                        StringResources.ErrorMessageReferenceNotFound,
                        EntityCategory.StatusPalette.ToString(),
                        string.Join(", ", statusPaletteIds.Except(resultStatusPalettes.Select(p => p.Id), StringComparer.OrdinalIgnoreCase))));
                }
            }
            else
            {
                logger.LogInformation($"StatusPaletteConfig is null in wrapper. Start to read StatusPaletteConfig on parent entities.");

                switch (wrapper.Type)
                {
                    case DHControlBaseWrapperDerivedTypes.Node:
                        var node = (DHControlNodeWrapper)wrapper;
                        var parentNode = node.GroupId == null ? null : await dHControlRepository.GetByIdAsync(node.GroupId).ConfigureAwait(false);
                        if (parentNode == null)
                        {
                            logger.LogInformation($"Parent node is null. Failed to read StatusPaletteConfig for parent entities.");

                            throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageMissingStatusPaletteConfig));
                        }
                        await this.ValidateStatusPaletteConfig(parentNode).ConfigureAwait(false);
                        break;
                    case DHControlBaseWrapperDerivedTypes.Group:
                        throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageMissingStatusPaletteConfig));
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private async Task<DHControlBaseWrapper> ReadEntityScheduleAsync(DHControlBaseWrapper entity)
        {
            if (entity.Type == DHControlBaseWrapperDerivedTypes.Node)
            {
                var node = (DHControlNodeWrapper)entity;

                var scheduleId = node.ScheduleId;

                if (!string.IsNullOrEmpty(scheduleId))
                {
                    logger.LogInformation($"Reading schedule for control node {node.Id} with schedule id {scheduleId}");

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
                    logger.LogInformation($"Creating schedule for control node {node.Id}");

                    var scheduleWrapper = new DHControlScheduleStoragePayloadWrapper([]);
                    scheduleWrapper.Properties = schedule;
                    scheduleWrapper.Type = DHControlScheduleType.ControlNode;

                    var scheduleEntity = await scheduleService.CreateScheduleAsync(scheduleWrapper, entity.Id).ConfigureAwait(false);
                    node.ScheduleId = scheduleEntity.Id;

                    logger.LogInformation($"Created schedule for control node {node.Id} with schedule id {scheduleEntity.Id}");
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
                    logger.LogInformation($"Updating schedule for control node {existNode.Id}");

                    var scheduleWrapper = new DHControlScheduleStoragePayloadWrapper([]);
                    scheduleWrapper.Properties = schedule;
                    scheduleWrapper.Type = DHControlScheduleType.ControlNode;

                    if (existNode.ScheduleId != null)
                    {
                        logger.LogInformation($"Existing schedule ID {existNode.ScheduleId} Start updating schedule for control node {existNode.Id}");

                        scheduleWrapper.Id = existNode.ScheduleId;
                        await scheduleService.UpdateScheduleAsync(scheduleWrapper, existEntity.Id).ConfigureAwait(false);

                        logger.LogInformation($"Updated schedule for control node {existNode.Id} with schedule id {existNode.ScheduleId}");
                    }
                    else
                    {
                        logger.LogInformation($"No existing schedule ID. Creating schedule for control node {existNode.Id}");

                        var scheduleEntity = await scheduleService.CreateScheduleAsync(scheduleWrapper, existEntity.Id).ConfigureAwait(false);
                        newNode.ScheduleId = scheduleEntity.Id;

                        logger.LogInformation($"Created schedule for control node {existNode.Id} with schedule id {scheduleEntity.Id}");
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
                    logger.LogInformation($"Deleting schedule for control node {node.Id} with schedule id {scheduleId}");

                    await scheduleService.DeleteScheduleAsync(scheduleId).ConfigureAwait(false);
                }
            }
        }
    }
}
