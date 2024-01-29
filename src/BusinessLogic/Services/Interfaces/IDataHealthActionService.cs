// <copyright file="IDomainService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces
{
    using Microsoft.Purview.DataEstateHealth.Models;
    using System.Threading.Tasks;

    public interface IDataHealthActionService
    {
        Task EnumerateActionsAsync();

        Task CreateActionsAsync(DataHealthActionModel entity);
    }
}
