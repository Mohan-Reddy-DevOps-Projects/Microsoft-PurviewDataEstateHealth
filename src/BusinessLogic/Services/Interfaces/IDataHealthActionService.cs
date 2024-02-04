// <copyright file="IDomainService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces
{
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDataHealthActionService
    {
        Task<IEnumerable<DataHealthActionWrapper>> EnumerateActionsAsync();

        Task<DataHealthActionWrapper> CreateActionsAsync(DataHealthActionWrapper entity);

        Task<DataHealthActionWrapper> GetActionByIdAsync(string actionId);
    }
}
