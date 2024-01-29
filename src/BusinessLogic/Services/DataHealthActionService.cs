// <copyright file="ActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.Models;
    using System;
    using System.Threading.Tasks;

    public class DataHealthActionService : IDataHealthActionService
    {
        private readonly DataHealthActionRepository dataHealthActionRepository;
        public DataHealthActionService(DataHealthActionRepository dataHealthActionRepository)
        {
            this.dataHealthActionRepository = dataHealthActionRepository;
        }

        public Task EnumerateActionsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task CreateActionsAsync(DataHealthActionModel entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            await this.dataHealthActionRepository.AddAsync(entity).ConfigureAwait(false);
        }
    }
}
