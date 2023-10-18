// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Add logger services
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Add the logger services to IServiceCollection
    /// </summary>
    /// <param name="serviceCollection"></param>
    public static IServiceCollection AddLogger(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDataEstateHealthLogger, DataEstateHealthLogger>();
        serviceCollection.AddScoped<IDataEstateHealthRequestLogger, DataEstateHealthLogger>();

        return serviceCollection;
    }
}
