// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using System.Net;

internal static class ExceptionConverter
{
    /// <summary>
    /// Convert an exception to an error model.
    /// </summary>
    /// <param name="ex">The original exception.</param>
    /// <param name="statusCode">The terminal status code.</param>
    /// <param name="envConfig">The environment configuration.</param>
    /// <param name="requestHeaderContext"></param>
    /// <returns></returns>
    public static ErrorModel CreateErrorModel(Exception ex, HttpStatusCode statusCode, EnvironmentConfiguration envConfig, IRequestHeaderContext requestHeaderContext)
    {
        ServiceException serviceException = GetKnownException(ex);
        if (serviceException != null)
        {
            return CreateKnownError(serviceException, statusCode);
        }
        else if (ex == null || !envConfig.IsDevelopmentEnvironment())
        {
            // by default do not throw unhandled exceptions to customer.
            return new(HttpStatusCode.InternalServerError.ToString(), "Unknown error")
            {
                Target = $"correlation ID: {requestHeaderContext.CorrelationId}",
            };
        }
        else
        {
            // allow unknown exceptions to be throw in dev environments.
            return CreateDetailedError(ex, statusCode);
        }
    }

    /// <summary>
    /// Cast the exception as a known exception otherwise null.
    /// </summary>
    /// <param name="ex">The original exception.</param>
    /// <returns></returns>
    public static ServiceException GetKnownException(Exception ex)
    {
        ServiceException serviceException = ex as ServiceException;
        serviceException ??= ex?.InnerException as ServiceException;

        return serviceException;
    }

    /// <summary>
    /// Creates an error model for a known exception
    /// </summary>
    /// <param name="serviceException">The known exception.</param>
    /// <param name="statusCode">The terminal status code</param>
    /// <returns></returns>
    public static ErrorModel CreateKnownError(ServiceException serviceException, HttpStatusCode statusCode)
    {
        ErrorModel error = new(serviceException.ServiceError.Code.ToString(), serviceException.Message);
        if (statusCode >= HttpStatusCode.InternalServerError)
        {
            return error;
        }

        List<ErrorModel> details = new();
        Exception innerException = serviceException.InnerException;
        while (innerException != null)
        {
            details.Add(new ErrorModel(serviceException.ServiceError.Code.ToString(), serviceException.ServiceError.Message));

            innerException = innerException.InnerException;
        }

        error.Details = details.ToArray();

        return error;
    }

    /// <summary>
    /// Creates an error model with full inner exception details. 
    /// </summary>
    /// <param name="exception">The known exception.</param>
    /// <param name="statusCode">The terminal status code.</param>
    /// <returns></returns>
    public static ErrorModel CreateDetailedError(Exception exception, HttpStatusCode statusCode)
    {
        ErrorModel error = new(statusCode.ToString(), exception.Message)
        {
            Target = exception.StackTrace
        };

        List<ErrorModel> details = new();
        Exception innerException = exception.InnerException;
        while (innerException != null)
        {
            details.Add(new(statusCode.ToString(), exception.Message)
            {
                Target = innerException.StackTrace
            });

            innerException = innerException.InnerException;
        }

        error.Details = details.ToArray();

        return error;
    }

    /// <summary>
    /// Create the status code to be returned for client requested.
    /// If the exception is a known exception a specific status code will be given, otherwise, an internal server error.
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static HttpStatusCode GetHttpStatusCode(Exception ex)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;

        ServiceException serviceException = ex as ServiceException;
        serviceException ??= ex?.InnerException as ServiceException;
        if (serviceException != null)
        {
            statusCode = GetHttpStatusCode(serviceException);
        }

        return statusCode;
    }

    /// <summary>
    /// Gets the HTTP status code per exception
    /// </summary>
    /// <param name="ex">The ex.</param>
    /// <returns></returns>
    public static HttpStatusCode GetHttpStatusCode(ServiceException ex)
    {
        return ex.ServiceError.Category switch
        {
            ErrorCategory.ResourceNotFound => HttpStatusCode.NotFound,
            ErrorCategory.Conflict => HttpStatusCode.Conflict,
            ErrorCategory.AuthenticationError => HttpStatusCode.Unauthorized,
            ErrorCategory.InputError => HttpStatusCode.BadRequest,
            ErrorCategory.DownStreamError => HttpStatusCode.BadGateway,
            ErrorCategory.Forbidden => HttpStatusCode.Forbidden,
            ErrorCategory.TooManyRequests => HttpStatusCode.TooManyRequests,
            _ => HttpStatusCode.InternalServerError,
        };
    }
}
