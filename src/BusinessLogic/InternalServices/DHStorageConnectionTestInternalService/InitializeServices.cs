// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

    public static class InitializeDHStorageConnectionServices
    {
        public static void SetupDHStorageConnectionTestInternalServices(this IServiceCollection services)
        {
            services.AddScoped<DHStorageConnectionTestInternalService>();
            services.AddScoped<DHStorageFabricConnectionTest>();
            services.AddScoped<DHStorageADLSGen2ConnectionTest>();
        }
    }
}
