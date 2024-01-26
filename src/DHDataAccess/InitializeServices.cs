// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
    using Microsoft.Purview.DataEstateHealth.Models.v2.DataAccess;

    public static class InitializeServices
    {
        public static void SetupDHDataAccessServices(this IServiceCollection services)
        {
            services.AddDbContext<CosmosDBContext>();

            services.AddScoped<DHSimpleRuleRepository>();
        }
    }
}
