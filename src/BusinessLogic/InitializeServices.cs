// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services.Interfaces;

    public static class InitializeServices
    {
        public static void SetupBusinessLogicServices(this IServiceCollection builder)
        {
            builder.AddSingleton<IActionService, ActionService>();
        }
    }
}
