// <copyright file="DHActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper;
    using Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities.ObligationHelper.Interfaces;
    using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
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
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;

    public class DHActionService(
        DHActionRepository dataHealthActionRepository,
        DHActionInternalService dHActionInternalService,
        IRequestHeaderContext requestHeaderContext,
        IDataEstateHealthRequestLogger logger,
        EnvironmentConfiguration environmentConfiguration
        )
    {
        public async Task<IBatchResults<DataHealthActionWrapper>> EnumerateActionsAsync(CosmosDBQuery<ActionsFilter> query)
        {
            using (logger.LogElapsed($"Start to enum actions"))
            {
                query.Filter = this.BuildEnumPermissionFilter(query.Filter);
                return await dataHealthActionRepository.GetActionsByFilterAsync(query);
            }
        }

        public async Task<ActionFacets> GetActionFacetsAsync(ActionsFilter filters, ActionFacets facets)
        {
            using (logger.LogElapsed($"Start to get action facets"))
            {
                filters = this.BuildEnumPermissionFilter(filters);
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
                query.Filter = this.BuildEnumPermissionFilter(query.Filter);
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

                var hasDHWritePermission = this.CheckObligation(GovernancePermissions.DgHealthWrite, ObligationContainerTypes.DataGovernanceApp, requestHeaderContext.TenantId.ToString(), EntityCategory.Action);
                var permissionMap = new Dictionary<DataHealthActionTargetEntityType, string>
                {
                    { DataHealthActionTargetEntityType.DataProduct, GovernancePermissions.DataProductWrite },
                    { DataHealthActionTargetEntityType.DataAsset, GovernancePermissions.DataProductWrite },
                    { DataHealthActionTargetEntityType.DataQualityAsset, GovernancePermissions.ObserverWrite },
                    { DataHealthActionTargetEntityType.BusinessDomain, GovernancePermissions.BusinessDomainWrite },
                    { DataHealthActionTargetEntityType.GlossaryTerm, GovernancePermissions.TermWrite }
                };

                if (!hasDHWritePermission && permissionMap.TryGetValue(action.TargetEntityType, out var permissionName))
                {
                    this.CheckObligation(permissionName, ObligationContainerTypes.DataGovernanceScope, action.DomainId, EntityCategory.Action, true);
                }
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
                var existAction = await this.GetExistedAction(actionId).ConfigureAwait(false);

                var hasDHReadPermission = this.CheckObligation(GovernancePermissions.DgHealthRead, ObligationContainerTypes.DataGovernanceApp, requestHeaderContext.TenantId.ToString(), EntityCategory.Action);
                var permissionMap = new Dictionary<DataHealthActionTargetEntityType, string>
                {
                    { DataHealthActionTargetEntityType.DataProduct, GovernancePermissions.DataProductRead },
                    { DataHealthActionTargetEntityType.DataAsset, GovernancePermissions.DataProductRead },
                    { DataHealthActionTargetEntityType.DataQualityAsset, GovernancePermissions.ObserverRead },
                    { DataHealthActionTargetEntityType.BusinessDomain, GovernancePermissions.BusinessDomainRead },
                    { DataHealthActionTargetEntityType.GlossaryTerm, GovernancePermissions.TermRead }
                };

                if (!hasDHReadPermission && permissionMap.TryGetValue(existAction.TargetEntityType, out var permissionName))
                {
                    var hasPermission = this.CheckObligation(permissionName, ObligationContainerTypes.DataGovernanceScope, existAction.DomainId, EntityCategory.Action);
                    if (!hasPermission)
                    {
                        throw new EntityForbiddenException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageForbiddenReadMessage, EntityCategory.Action, existAction.DomainId));
                    }
                }
                return existAction;
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

        internal ActionsFilter BuildEnumPermissionFilter(ActionsFilter? filter)
        {
            filter ??= new ActionsFilter();
            var hasDHReadPermission = this.CheckObligation(GovernancePermissions.DgHealthRead, ObligationContainerTypes.DataGovernanceApp, requestHeaderContext.TenantId.ToString(), EntityCategory.Action);
            if (!hasDHReadPermission)
            {
                filter.PermissionObligations = new Dictionary<DataHealthActionTargetEntityType, List<Obligation>>
                    {
                        { DataHealthActionTargetEntityType.BusinessDomain, this.GetObligationQueryFilter(ObligationContainerTypes.DataGovernanceScope, GovernancePermissions.BusinessDomainRead) },
                        { DataHealthActionTargetEntityType.DataProduct, this.GetObligationQueryFilter(ObligationContainerTypes.DataGovernanceScope, GovernancePermissions.DataProductRead) },
                        { DataHealthActionTargetEntityType.DataQualityAsset, this.GetObligationQueryFilter(ObligationContainerTypes.DataGovernanceScope, GovernancePermissions.ObserverRead) },
                        { DataHealthActionTargetEntityType.GlossaryTerm, this.GetObligationQueryFilter(ObligationContainerTypes.DataGovernanceScope, GovernancePermissions.TermRead) },
                };
            }
            return filter;
        }

        internal List<Obligation> GetObligationQueryFilter(string obligationContainerType, string permission)
        {
            return this.GetObligationQueryFilter(new List<string> { obligationContainerType }, permission);
        }

        internal List<Obligation> GetObligationQueryFilter(List<string> obligationContainerTypes, string permission)
        {
            if (!this.EnableObligationCheck())
            {
                // Permit all by default.
                return new List<Obligation>()
                    {
                        new Obligation()
                        {
                            Type = ObligationType.NotApplicable,
                        }
                    };
            }
            var obligationDict = requestHeaderContext.Obligations;

            // TODO: need to remove this log.
            logger.LogInformation($"Obligation: {JsonConvert.SerializeObject(obligationDict)}");

            var obligations = ObligationHelper.GetObligations(obligationDict, obligationContainerTypes, permission);

            return obligations;
        }

        internal bool CheckObligation(string permission, string obligationContainerType, string domainId, EntityCategory entityType, bool throwException = false)
        {
            return this.CheckObligation(permission, new List<string> { obligationContainerType }, domainId, entityType, throwException);
        }

        internal bool CheckObligation(string permission, List<string> obligationContainerTypes, string domainId, EntityCategory entityType, bool throwException = false)
        {
            using (logger.LogElapsed("Check obligation."))
            {
                if (!this.EnableObligationCheck())
                {
                    logger.LogInformation($"Obligation passed because it is not enabled.");
                    return true;
                }

                var obligationDict = requestHeaderContext.Obligations;

                bool isAccessAllowed = ObligationHelper.IsAccessAllowed(obligationDict, obligationContainerTypes, permission, domainId);
                if (!isAccessAllowed)
                {
                    logger.LogInformation($"Obligation denied.");
                    if (throwException)
                    {
                        throw new EntityForbiddenException(String.Format(CultureInfo.InvariantCulture, StringResources.ErrorMessageForbiddenMessage, entityType, domainId));
                    }
                    return false;
                }
                logger.LogInformation($"Obligation passed.");
                return isAccessAllowed;
            }
        }
        private bool EnableObligationCheck()
        {
            return environmentConfiguration.IsDogfoodEnvironment();
        }
    }
}
