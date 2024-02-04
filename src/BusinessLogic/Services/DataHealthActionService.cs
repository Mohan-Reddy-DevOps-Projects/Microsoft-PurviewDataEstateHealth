// <copyright file="ActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class DataHealthActionService : IDataHealthActionService
    {
        private readonly DataHealthActionRepository dataHealthActionRepository;
        public DataHealthActionService(DataHealthActionRepository dataHealthActionRepository)
        {
            this.dataHealthActionRepository = dataHealthActionRepository;
        }

        public async Task<IEnumerable<DataHealthActionWrapper>> EnumerateActionsAsync()
        {
            return await this.dataHealthActionRepository.GetAllAsync();
        }

        public async Task<DataHealthActionWrapper> CreateActionsAsync(DataHealthActionWrapper entity)
        {
            //TODO: aggregate acitons
            await this.dataHealthActionRepository.AddAsync(entity).ConfigureAwait(false);
            return entity;
        }

        public async Task<DataHealthActionWrapper> GetActionByIdAsync(string actionId)
        {
            if (actionId == null)
            {
                throw new ArgumentNullException(nameof(actionId));
            }

            var result = await this.dataHealthActionRepository.GetByIdAsync(actionId).ConfigureAwait(false);
            if (result == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Action.ToString(), actionId));
            }
            return result;
        }
    }
}
