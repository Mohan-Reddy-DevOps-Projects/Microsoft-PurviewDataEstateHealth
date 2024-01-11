// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using System.Text;
using System.Net.Http.Headers;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Data.SqlClient;

internal static class ExceptionHandler
{
    private static readonly JsonSerializerSettings jsonSerializerSettings = new()
    {
        MaxDepth = 128,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    /// <summary>
    /// Handles converting the original exception to a known exception and writes the error model to the response.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logger"></param>
    /// <param name="envConfig"></param>
    /// <param name="requestContextAccessor"></param>
    /// <returns></returns>
    public static async Task HandleException(HttpContext context, IDataEstateHealthRequestLogger logger, EnvironmentConfiguration envConfig, IRequestContextAccessor requestContextAccessor)
    {
        IExceptionHandlerFeature contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature == null)
        {
            return;
        }
       
        Exception exception = GetSqlDataNotFoundException(contextFeature.Error);
        HttpStatusCode statusCode = ExceptionConverter.GetHttpStatusCode(exception);

        ErrorResponseModel errorResponse = new()
        {
            Error = ExceptionConverter.CreateErrorModel(exception, statusCode, envConfig, requestContextAccessor.GetRequestContext())
        };

        await ModifyHttpResponse(context, statusCode, errorResponse);

        logger.LogError($"ExceptionMiddleware| Http context response has been written with status: {statusCode}.", contextFeature.Error);
    }

    /// <summary>
    /// Writes the error model to the response.
    /// </summary>
    /// <param name="context">The http context.</param>
    /// <param name="statusCode">The terminal status code.</param>
    /// <param name="errorResponse">The error model.</param>
    /// <returns></returns>
    private async static Task ModifyHttpResponse(HttpContext context, HttpStatusCode statusCode, ErrorResponseModel errorResponse)
    {
        // Build HTTP response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = new MediaTypeHeaderValue("application/json").ToString();
        await context.Response.WriteAsync(JsonConvert.SerializeObject(
            errorResponse,
            jsonSerializerSettings), Encoding.UTF8);
    }

    private static Exception GetSqlDataNotFoundException(Exception ex)
    {
        SqlException sqlException = ex.InnerException as SqlException;
        if (sqlException.Number == 13807 || (sqlException.InnerException is SqlException innerSqlException && innerSqlException.Number == 13807))
        {
            return new ServiceError(ErrorCategory.ResourceNotFound, ErrorCode.AsyncOperation_ResultNotFound.Code, "The requested data could not be found.").ToException();
        }

        return ex;
    }
}

