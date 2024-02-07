// <copyright file="IDataHealthActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces
{
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction.Models;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDataHealthActionService
    {
        Task<IEnumerable<DataHealthActionWrapper>> EnumerateActionsAsync();

        Task<IEnumerable<GroupedActions>> EnumerateActionsByGroupAsync(string groupBy);

        Task<DataHealthActionWrapper> CreateActionsAsync(DataHealthActionWrapper entity);

        Task<DataHealthActionWrapper> UpdateActionAsync(string actionId, DataHealthActionWrapper entity);

        Task<DataHealthActionWrapper> GetActionByIdAsync(string actionId);
    }
}
