// <copyright file="ActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHActionService(DHActionRepository dataHealthActionRepository, IRequestHeaderContext requestHeaderContext)
    {
        public async Task<IEnumerable<DataHealthActionWrapper>> EnumerateActionsAsync()
        {
            return await dataHealthActionRepository.GetAllAsync();
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
            return await dataHealthActionRepository.EnumerateActionsByGroupAsync(groupBy);
        }

        public async Task<IEnumerable<DataHealthActionWrapper>> CreateActionsAsync(IEnumerable<DataHealthActionWrapper> actions)
        {
            if (actions == null)
            {
                throw new ArgumentNullException(nameof(actions));
            }

            actions.ForEach((action) =>
            {
                action.Validate();
                action.NormalizeInput();
            });
            //TODO: Consider the perfermance while get all actions.
            var existedActions = await dataHealthActionRepository.GetAllAsync();

            var createActionList = new List<DataHealthActionWrapper>();
            var updateActionList = new List<DataHealthActionWrapper>();
            foreach (var action in actions)
            {
                var matchedExistedAction = existedActions.FirstOrDefault(v =>
                    v.Category == action.Category &&
                    v.FindingType == action.FindingType &&
                    v.FindingSubType == action.FindingSubType &&
                    v.FindingId == action.FindingId &&
                    v.TargetEntityType == action.TargetEntityType &&
                    v.TargetEntityId == action.TargetEntityId &&
                    (v.Status == DataHealthActionStatus.NotStarted || v.Status == DataHealthActionStatus.InProgress));
                if (matchedExistedAction != null)
                {
                    matchedExistedAction.SystemInfo.onHint();
                    updateActionList.Add(matchedExistedAction);
                }
                else
                {
                    action.Id = Guid.NewGuid().ToString();
                    action.onCreate();
                    createActionList.Add(action);
                }
            }
            if (createActionList.Count > 0)
            {
                await dataHealthActionRepository.AddAsync(createActionList);
            }

            if (updateActionList.Count > 0)
            {
                await dataHealthActionRepository.UpdateAsync(updateActionList);
            }
            return createActionList.Concat(updateActionList);
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

            action.OnReplace(existedAction);

            var clientObjectId = requestHeaderContext.ClientObjectId?.ToString();
            existedAction.SystemInfo.OnModify(clientObjectId);

            await dataHealthActionRepository.UpdateAsync(action).ConfigureAwait(false);
            return action;
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
            var result = await dataHealthActionRepository.GetByIdAsync(actionId).ConfigureAwait(false);
            if (result == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Action.ToString(), actionId));
            }
            return result;
        }
    }
}
