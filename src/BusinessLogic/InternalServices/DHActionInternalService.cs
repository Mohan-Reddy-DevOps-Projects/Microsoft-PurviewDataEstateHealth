// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
    using Microsoft.Purview.DataEstateHealth.DHModels.Queries;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHActionInternalService(DHActionRepository dataHealthActionRepository, IDataEstateHealthRequestLogger logger)
    {
        public async Task<IEnumerable<DataHealthActionWrapper>> BatchCreateOrUpdateActionsAsync(IEnumerable<DataHealthActionWrapper> actions)
        {
            using (logger.LogElapsed($"Start to create actions, internal call, number: {actions.Count()}"))
            {
                if (actions == null)
                {
                    throw new ArgumentNullException(nameof(actions));
                }

                actions.ForEach((action) =>
                {
                    action.Status = action.Status == null ? DataHealthActionStatus.NotStarted : action.Status;
                    action.Validate();
                    action.NormalizeInput();
                });

                var query = new CosmosDBQuery<ActionsFilter>()
                {
                    Filter = new ActionsFilter()
                    {
                        Status = [DataHealthActionStatus.NotStarted, DataHealthActionStatus.InProgress]
                    },
                };
                var existedActions = await dataHealthActionRepository.GetActionsByFilterAsync(query, true);

                var createActionList = new List<DataHealthActionWrapper>();
                var updateActionList = new List<DataHealthActionWrapper>();
                foreach (var action in actions)
                {
                    var matchedExistedAction = existedActions.Results?.FirstOrDefault(v =>
                        v.Category == action.Category &&
                        v.FindingType == action.FindingType &&
                        v.FindingSubType == action.FindingSubType &&
                        v.FindingId == action.FindingId &&
                        v.TargetEntityType == action.TargetEntityType &&
                        v.TargetEntityId == action.TargetEntityId);
                    if (matchedExistedAction != null)
                    {
                        if (action.Status == DataHealthActionStatus.Resolved)
                        {
                            matchedExistedAction.OnResolve(DHModelConstants.SYSTEM_USER);
                        }
                        else
                        {
                            matchedExistedAction.SystemInfo.OnHint();
                        }
                        updateActionList.Add(matchedExistedAction);
                    }
                    else
                    {
                        action.OnCreate(DHModelConstants.SYSTEM_USER);
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
    }
}
