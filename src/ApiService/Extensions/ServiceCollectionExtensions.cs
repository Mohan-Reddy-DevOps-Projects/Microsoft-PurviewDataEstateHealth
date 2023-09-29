// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Logger;

/// <summary>
/// Extension methods on ServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add logger related classes
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLogger(this IServiceCollection services)
    {
        services.AddScoped<IRequestHeaderContext, RequestHeaderContext>();
        services.AddSingleton<IDataEstateHealthLogger, DataEstateHealthLogger>();

        return services;
    }
}
