// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;

internal static class ExceptionMiddlewareExtensions
{
    /// <summary>
    /// Adds the exception handler middleware to the pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="logger"></param>
    /// <param name="envConfig"></param>
    /// <param name="requestContextAccessor"></param>
    public static void ConfigureExceptionHandler(this IApplicationBuilder app, IDataEstateHealthRequestLogger logger, IOptions<EnvironmentConfiguration> envConfig, IRequestContextAccessor requestContextAccessor)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                await ExceptionHandler.HandleException(context, logger, envConfig.Value, requestContextAccessor);
            });
        });
    }
}
