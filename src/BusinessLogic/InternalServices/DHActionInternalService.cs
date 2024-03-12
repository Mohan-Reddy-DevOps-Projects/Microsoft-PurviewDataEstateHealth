// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Azure.Purview.DataEstateHealth.Models;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DHActionInternalService(DHActionRepository dataHealthActionRepository, IRequestHeaderContext requestHeaderContext, IDataEstateHealthRequestLogger logger)
    {
        public async Task<IEnumerable<DataHealthActionWrapper>> CreateActionsAsync(IEnumerable<DataHealthActionWrapper> actions)
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
                        matchedExistedAction.SystemInfo.OnHint();
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
    }
}
