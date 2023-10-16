// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides behavior on the data access layer level.
/// </summary>
public static class DataAccessLayer
{
    /// <summary>
    /// Initializes the data access layer.
    /// </summary>
    /// <param name="services">Gives the data access layer a chance to configure its dependency injection.</param>
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
    {
        _ = services.AddScoped<IDataEstateHealthSummaryRepository, DataEstateHealthSummaryRepository>();
        _ = services.AddScoped<IHealthActionRepository, HealthActionRepository>();
        _ = services.AddScoped<IHealthScoreRepository, HealthScoreRepository>();
        _ = services.AddScoped<IBusinessDomainRepository, BusinessDomainRepository>();

        return services;
    }
}
