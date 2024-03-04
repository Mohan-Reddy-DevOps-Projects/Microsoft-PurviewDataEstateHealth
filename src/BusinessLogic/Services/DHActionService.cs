// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHActionService(DHActionRepository dataHealthActionRepository, IRequestHeaderContext requestHeaderContext, IDataEstateHealthRequestLogger logger)
    {
        public async Task<IEnumerable<DataHealthActionWrapper>> EnumerateActionsAsync(CosmosDBQuery<ActionsFilter> query)
        {
            using (logger.LogElapsed($"Start to enum actions"))
            {
                return await dataHealthActionRepository.GetActionsByFilterAsync(query);
            }
        }

        public async Task<ActionFacets> GetActionFacetsAsync(ActionsFilter filters, ActionFacets facets)
        {
            using (logger.LogElapsed($"Start to get action facets"))
            {
                return await dataHealthActionRepository.GetActionFacetsAsync(filters, facets);
            }
        }

        public async Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(CosmosDBQuery<ActionsFilter> query, string groupBy)
        {
            using (logger.LogElapsed($"Start to enum grouped actions"))
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
                    logger.LogInformation($"The value of {nameof(groupBy)} is not supported");
                    throw new UnsupportedParamException($"The value of {nameof(groupBy)} is not supported");
                }
                return await dataHealthActionRepository.EnumerateActionsByGroupAsync(query, groupBy);
            }
        }

        public async Task<IEnumerable<DataHealthActionWrapper>> CreateActionsAsync(IEnumerable<DataHealthActionWrapper> actions)
        {
            using (logger.LogElapsed($"Start to create actions"))
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
                        action.OnCreate(requestHeaderContext.ClientObjectId);
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
                logger.LogInformation($"Finished creating actions. Created {createActionList.Count}, Updated {updateActionList.Count}");
                return createActionList.Concat(updateActionList);
            }
        }

        public async Task<DataHealthActionWrapper> UpdateActionAsync(string actionId, DataHealthActionWrapper action)
        {
            using (logger.LogElapsed($"Start to update actions, actionId: {actionId}"))
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

                logger.LogInformation($"Update action, actionId: {actionId}");

                var existedAction = await this.GetExistedAction(actionId).ConfigureAwait(false);

                if (existedAction == null)
                {
                    logger.LogInformation($"Can't find the existing action, actionId: {actionId}");
                    throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Action.ToString(), actionId));
                }

                action.OnUpdate(existedAction, requestHeaderContext.ClientObjectId);

                await dataHealthActionRepository.UpdateAsync(action).ConfigureAwait(false);
                return action;
            }
        }

        public async Task<DataHealthActionWrapper> GetActionByIdAsync(string actionId)
        {
            using (logger.LogElapsed($"Start to get action by ID, actionId: {actionId}"))
            {
                if (actionId == null)
                {
                    throw new ArgumentNullException(nameof(actionId));
                }

                return await this.GetExistedAction(actionId).ConfigureAwait(false);
            }
        }

        private async Task<DataHealthActionWrapper> GetExistedAction(string actionId)
        {
            var result = await dataHealthActionRepository.GetByIdAsync(actionId).ConfigureAwait(false);
            if (result == null)
            {
                logger.LogInformation($"Can't find the existing action, actionId: {actionId}");
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Action.ToString(), actionId));
            }
            return result;
        }
    }
}
