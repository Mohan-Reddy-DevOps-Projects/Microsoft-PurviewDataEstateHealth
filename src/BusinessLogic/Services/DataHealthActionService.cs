// <copyright file="ActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    public class DataHealthActionService : IDataHealthActionService
    {
        private readonly DataHealthActionRepository dataHealthActionRepository;
        private readonly IRequestHeaderContext requestHeaderContext;
        public DataHealthActionService(DataHealthActionRepository dataHealthActionRepository, IRequestHeaderContext requestHeaderContext)
        {
            this.dataHealthActionRepository = dataHealthActionRepository;
            this.requestHeaderContext = requestHeaderContext;
        }

        public async Task<IEnumerable<DataHealthActionWrapper>> EnumerateActionsAsync()
        {
            return await this.dataHealthActionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(string groupBy)
        {
            HashSet<string> allowedKeys = new HashSet<string>
            {
                DataHealthActionWrapper.keyFindingType,
                DataHealthActionWrapper.keyFindingSubType,
                DataHealthActionWrapper.keyFindingName,
                DataHealthActionWrapper.keySeverity
            };
            if (!allowedKeys.Contains(groupBy))
            {
                throw new UnsupportedParamException($"The value of {nameof(groupBy)} is not supported");
            }
            return await this.dataHealthActionRepository.EnumerateActionsByGroupAsync(groupBy);
        }

        public async Task<DataHealthActionWrapper> CreateActionsAsync(DataHealthActionWrapper action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            action.Validate();
            action.NormalizeInput();

            var existedAction = await this.dataHealthActionRepository.GetActionByFilterAsync(
                action.Category, action.FindingType, action.FindingSubType, action.FindingId, action.TargetEntityType, action.TargetEntityId);

            if (existedAction != null && existedAction.Status == DataHealthActionStatus.Active)
            {
                existedAction.SystemInfo.onHint();
                await this.dataHealthActionRepository.UpdateAsync(existedAction).ConfigureAwait(false);
                return existedAction;
            }
            else
            {
                action.onCreate();
                await this.dataHealthActionRepository.AddAsync(action).ConfigureAwait(false);
                return action;
            }

        }

        public async Task<DataHealthActionWrapper> UpdateActionAsync(string actionId, DataHealthActionWrapper action)
        {
            if (actionId == null)
            {
                throw new ArgumentNullException(nameof(actionId));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (!string.Equals(actionId, action.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new EntityValidationException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageUpdateEntityIdNotMatch, EntityCategory.Action.ToString(), action.Id, actionId));
            }

            action.Validate();
            action.NormalizeInput();

            var existedAction = await this.GetExistedAction(actionId).ConfigureAwait(false);

            // TOOD(han): replace with a new instance of wrapper will fail in CommonRepository now, so update the existed one directly
            //action.OnReplace(existedAction);
            existedAction.Status = action.Status;
            existedAction.AssignedTo = action.AssignedTo;

            var clientObjectId = this.requestHeaderContext.ClientObjectId?.ToString();
            existedAction.SystemInfo.OnModify(clientObjectId);

            await this.dataHealthActionRepository.UpdateAsync(existedAction).ConfigureAwait(false);
            return existedAction;
        }

        public async Task<DataHealthActionWrapper> GetActionByIdAsync(string actionId)
        {
            if (actionId == null)
            {
                throw new ArgumentNullException(nameof(actionId));
            }

            return await this.GetExistedAction(actionId).ConfigureAwait(false);
        }

        private async Task<DataHealthActionWrapper> GetExistedAction(string actionId)
        {
            var result = await this.dataHealthActionRepository.GetByIdAsync(actionId).ConfigureAwait(false);
            if (result == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Action.ToString(), actionId));
            }
            return result;
        }
    }
}
