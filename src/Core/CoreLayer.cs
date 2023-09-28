// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for setting up the core layer.
/// </summary>
public static class CoreLayer
{
    /// <summary>
    /// Initializes the core layer dependencies.
    /// </summary>
    /// <param name="services">Gives the core layer a chance to configure its dependency injection.</param>
    /// <param name="azureCredentials"></param>
    public static IServiceCollection AddCoreLayer(this IServiceCollection services, TokenCredential azureCredentials)
    {
        services.AddSingleton(azureCredentials);
        services.AddSingleton<IExceptionAdapterService, ExceptionAdapterService>();
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IStorageCredentialsProvider, StorageCredentialsProvider>();

        services.AddScoped<IApiClient, ApiClient>();
        services.AddScoped<IRequestHeaderContext, RequestHeaderContext>();

        return services;
    }
}
