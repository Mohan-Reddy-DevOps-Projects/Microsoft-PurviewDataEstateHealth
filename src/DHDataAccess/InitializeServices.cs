// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DataHealthAction;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

    public static class InitializeServices
    {
        public static void SetupDHDataAccessServices(this IServiceCollection services)
        {
            services.AddDbContext<ControlDBContext>();
            services.AddScoped<DHControlNodeRepository>();
            services.AddScoped<DHControlGroupRepository>();

            services.AddDbContext<ActionDBContext>();

            services.AddScoped<DataHealthActionRepository>();
        }
    }
}
