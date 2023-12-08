// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// A filter that handles exceptions thrown by the service.
/// </summary>
public class ServiceExceptionFilter : IExceptionFilter
{
    private readonly IDataEstateHealthRequestLogger logger;

    private readonly IExceptionAdapterService exceptionAdapter;

    private readonly IRequestHeaderContext requestHeaderContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceExceptionFilter" /> class.
    /// </summary>
    public ServiceExceptionFilter(
        IDataEstateHealthRequestLogger purviewShareRequestLogger,
        IExceptionAdapterService exceptionAdapter,
        IRequestHeaderContext requestHeaderContext)
    {
        this.logger = purviewShareRequestLogger;
        this.exceptionAdapter = exceptionAdapter;
        this.requestHeaderContext = requestHeaderContext;
    }

    /// <inheritdoc/>
    public void OnException(ExceptionContext context)
    {
        try
        {
            this.ProcessException(context);
        }
        catch (Exception exception)
        {
            this.logger.LogCritical("Unhandled error in processing exception", exception);

            var result = new DataEstateHealthError
            {
                Error = new DataEstateHealthErrorInfo(ErrorCode.Unknown.Code.ToString(), ErrorCode.Unknown.Message)
            };

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.HttpContext.Response.Headers.Add("x-ms-error-code", result.Error.Code);
            context.Result = new ObjectResult(result);
            context.ExceptionHandled = true;
        }
    }

    private void ProcessException(ExceptionContext context)
    {
        if (context == null)
        {
            return;
        }

        var statusCode = HttpStatusCode.InternalServerError;
        var errorInfo = new DataEstateHealthErrorInfo(ErrorCode.Unknown.Code.ToString(), ErrorCode.Unknown.Message);
        bool isServiceExceptionProcessed = false;

        if (context.Exception is ServiceException serviceException)
        {
            statusCode = this.exceptionAdapter.GetUserFacingStatusCode(serviceException);
            errorInfo = this.exceptionAdapter.GetUserFacingException(serviceException);
            isServiceExceptionProcessed = true;
        }
        else if (context.Exception is AggregateException aggregateException)
        {
            var currentException = aggregateException;
            while (currentException.InnerException != null)
            {
                if (currentException.InnerException is ServiceException innerServiceException)
                {
                    statusCode = this.exceptionAdapter.GetUserFacingStatusCode(innerServiceException);
                    errorInfo = this.exceptionAdapter.GetUserFacingException(innerServiceException);
                    isServiceExceptionProcessed = true;
                    break;
                }
                else if (currentException.InnerException is AggregateException innerAggregateException)
                {
                    currentException = innerAggregateException;
                }
                else
                {
                    break;
                }
            }
        }

        if (!isServiceExceptionProcessed && context.Exception is BadHttpRequestException badHttpRequestException)
        {
            ServiceException badHttpServiceException = new ServiceError(
                    ErrorCategory.InputError,
                    badHttpRequestException.StatusCode,
                    badHttpRequestException.Message)
                .ToException();

            statusCode = this.exceptionAdapter.GetUserFacingStatusCode(badHttpServiceException);
            errorInfo = this.exceptionAdapter.GetUserFacingException(badHttpServiceException);
        }

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            this.logger.LogCritical("Unhandled server error", context.Exception);
        }
        else
        {
            this.logger.LogError("Unhandled client error", context.Exception);
        }

        errorInfo.Target = $"correlation ID: {this.requestHeaderContext.CorrelationId}";

        context.HttpContext.Response.StatusCode = (int)statusCode;
        context.HttpContext.Response.Headers.Add("x-ms-error-code", errorInfo.Code);
        context.Result = new ObjectResult(new DataEstateHealthError { Error = errorInfo });
        context.ExceptionHandled = true;
    }
}
