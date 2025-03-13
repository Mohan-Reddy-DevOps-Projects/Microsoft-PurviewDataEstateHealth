// <copyright file="InitializeServices.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Interfaces;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
    using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

    public static class InitializeServices
    {
        public static void SetupBusinessLogicServices(this IServiceCollection services)
        {
            services.AddScoped<DHScheduleInternalService>();
            services.AddScoped<DHActionInternalService>();
            services.AddScoped<DHStatusPaletteInternalService>();
            services.SetupDHStorageConnectionTestInternalServices();

            services.AddScoped<DHActionService>();
            services.AddScoped<DHControlService>();
            services.AddScoped<DHDataEstateHealthService>();
            services.AddScoped<DHScheduleService>();
            services.AddScoped<DHStatusPaletteService>();
            services.AddScoped<DHAssessmentService>();
            services.AddScoped<DHScoreService>();
            services.AddScoped<DHMonitoringService>();
            services.AddScoped<DHTemplateService>();
            services.AddScoped<DHProvisionService>();
            services.AddScoped<DHAlertService>();
            services.AddScoped<DHStorageConfigService>();
            services.AddScoped<IDHAnalyticsScheduleService, DHAnalyticsScheduleService>();
        }
    }
}
