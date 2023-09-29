// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

/// <summary>
/// Extension methods for configuring the Api Service.
/// </summary>
public static class ApiService
{
    /// <summary>
    /// Initializes the api service.
    /// </summary>
    /// <param name="services">Gives the api service a chance to configure its dependency injection.</param>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services)
    {
        return services;
    }
}
