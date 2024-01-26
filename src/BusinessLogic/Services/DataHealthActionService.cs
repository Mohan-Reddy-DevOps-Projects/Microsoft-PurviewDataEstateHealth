// <copyright file="ActionService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services
{
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces;
    using System;
    using System.Threading.Tasks;

    public class DataHealthActionService : IDataHealthActionService
    {

        public DataHealthActionService()
        {

        }

        public Task EnumerateActionsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
