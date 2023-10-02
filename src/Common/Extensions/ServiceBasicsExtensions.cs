// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.DGP.ServiceBasics;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to configure service basics
/// </summary>
public static class ServiceBasicsExtensions
{
    private static readonly Assembly[] baseAssemblies = new Assembly[]
    {
        Assembly.Load("Microsoft.Azure.Purview.DataEstateHealth.Core"),
        Assembly.Load("Microsoft.Azure.Purview.DataEstateHealth.DataAccess"),
        Assembly.Load("Microsoft.Azure.Purview.DataEstateHealth.Models"),
    };

    /// <summary>
    /// Add the service basics configuration to IServiceCollection
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns>An <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddServiceBasicsForApiService(this IServiceCollection serviceCollection)
    {
        IEnumerable<Assembly> assemblies = baseAssemblies.Append(
            Assembly.Load("Microsoft.Azure.Purview.DataEstateHealth.ApiService"));
        return serviceCollection
                .ConfigureServiceBasics(new ServiceBasicsConfigurationOptions.Builder(assemblies).Build());
    }

    /// <summary>
    /// Add the service basics configuration to IServiceCollection
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <returns>An <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddServiceBasicsForWorkerService(this IServiceCollection serviceCollection)
    {
        return serviceCollection.ConfigureServiceBasics(
                new ServiceBasicsConfigurationOptions.Builder(
                    baseAssemblies).Build());
    }
}
