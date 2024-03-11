// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    public class DHActionService(DHActionRepository dataHealthActionRepository, DHActionInternalService dHActionInternalService, IRequestHeaderContext requestHeaderContext, IDataEstateHealthRequestLogger logger)
    {
        public async Task<IBatchResults<DataHealthActionWrapper>> EnumerateActionsAsync(CosmosDBQuery<ActionsFilter> query)
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
            var a = typeof(DataHealthActionWrapper).GetProperty("findingName")?.Name;
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
                return await dHActionInternalService.CreateActionsAsync(actions);
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

                action.Status = action.Status == null ? DataHealthActionStatus.NotStarted : action.Status;

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
