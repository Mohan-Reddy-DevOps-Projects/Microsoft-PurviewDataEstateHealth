// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Data.SqlClient;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

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

        Exception exception = ConvertKnownExceptions(contextFeature.Error);

        HttpStatusCode statusCode = ExceptionConverter.GetHttpStatusCode(exception);

        ErrorResponseModel errorResponse = new()
        {
            Error = ExceptionConverter.CreateErrorModel(exception, statusCode, envConfig, requestContextAccessor.GetRequestContext(), logger)
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

    private static Exception ConvertKnownExceptions(Exception ex)
    {
        // Handle SqlDataNotFoundException
        SqlException sqlException = ex.InnerException as SqlException;
        if (sqlException != null && (sqlException.Number == 13807 || (sqlException.InnerException is SqlException innerSqlException && innerSqlException.Number == 13807)))
        {
            return new ServiceError(ErrorCategory.ResourceNotFound, ErrorCode.AsyncOperation_ResultNotFound.Code, "The requested data could not be found.").ToException();
        }

        switch (ex)
        {
            case EntityNotFoundException:
                return new ExtendedServiceException(
                    ErrorCategory.ResourceNotFound,
                    ErrorResponseCode.DataHealthNotFoundError,
                    ex.Message
                    );
            case EntityConflictException:
                return new ExtendedServiceException(
                    ErrorCategory.Conflict,
                    ErrorResponseCode.DataHealthConflictError,
                    ex.Message
                );
            case EntityReferencedException:
                return new ExtendedServiceException(
                    ErrorCategory.Conflict,
                    ErrorResponseCode.DataHealthEntityReferencedError,
                    ex.Message
                );
            case EntityValidationException:
            case InvalidRequestException:
                return new ExtendedServiceException(
                    ErrorCategory.InputError,
                    ErrorResponseCode.DataHealthInvalidEntity,
                    ex.Message
                );
            case EntityForbiddenException:
                return new ExtendedServiceException(
                    ErrorCategory.Forbidden,
                    ErrorResponseCode.DataHealthForbiddenError,
                    ex.Message
                );
            default:
                return ex;
        }
    }
}

