// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;

internal class ExceptionAdapterService : IExceptionAdapterService
{
    public DataEstateHealthErrorInfoModel GetUserFacingException(ServiceException serviceException)
    {
        HttpStatusCode status = this.GetUserFacingStatusCode(serviceException);

        var errorInfo = new DataEstateHealthErrorInfoModel(
            ExceptionAdapterService.TranslateToUserFacingCode(serviceException.ServiceError.Code),
            this.IsClientError(serviceException)
                ? serviceException.Message
                : ErrorMessage.ServiceErrorMessage)
        {
            Target = this.IsClientError(serviceException)
                ? ""
                : ErrorMessage.ServiceErrorDetailsMessage
        };

        if (status != HttpStatusCode.InternalServerError)
        {
            Exception innerException = serviceException.InnerException;
            var innerErrorInfos = new List<DataEstateHealthErrorInfoModel>();

            while (innerException != null)
            {
                if (innerException is ServiceException innerServiceException)
                {
                    innerErrorInfos.Add(
                        new DataEstateHealthErrorInfoModel(
                            ExceptionAdapterService.TranslateToUserFacingCode(
                                innerServiceException.ServiceError.Code),
                            this.IsClientError(serviceException)
                                ? innerException.Message
                                : ErrorMessage.ServiceErrorMessage));
                }
                else
                {
                    innerErrorInfos.Add(
                        new DataEstateHealthErrorInfoModel(
                            HttpStatusCode.InternalServerError.ToString(),
                            "Unknown error"));
                }

                innerException = innerException.InnerException;
            }

            errorInfo.Details = innerErrorInfos.ToArray();
        }

        return errorInfo;
    }

    public HttpStatusCode GetUserFacingStatusCode(ServiceException serviceException)
    {
        switch (serviceException.ServiceError.Category)
        {
            case ErrorCategory.ResourceNotFound:
                return HttpStatusCode.NotFound;

            case ErrorCategory.Conflict:
                return HttpStatusCode.Conflict;

            case ErrorCategory.AuthenticationError:
                return HttpStatusCode.Unauthorized;

            /* ARM allows only certain status codes.
             RP contract violation: unexpected HTTP status code 403 'Forbidden'. 
             Status code is changed to 502 'BadGateway' to indicate service failure. 
             Please change status code to 'BadRequest', 'Conflict', 'PreconditionFailed' or as appropriate. 
             See https://aka.ms/rpcapi for more details.
            */
            case ErrorCategory.Forbidden:
                return HttpStatusCode.BadRequest;

            case ErrorCategory.InputError:
                return HttpStatusCode.BadRequest;

            case ErrorCategory.TooManyRequests:
                return (HttpStatusCode)429;

            case ErrorCategory.ConcurrencyMismatch:
                //TODO: Change this to 500 after handling retries in the repository layer
                return HttpStatusCode.Conflict;

            case ErrorCategory.DownStreamError:
            case ErrorCategory.ServiceError:
            default:
                return HttpStatusCode.InternalServerError;
        }
    }

    public DataEstateHealthErrorInfoModel GetDefaultUserFacingException()
    {
        return new DataEstateHealthErrorInfoModel(HttpStatusCode.InternalServerError.ToString(), "Unknown error");
    }

    public DataEstateHealthErrorInfoModel GetUserFacingHierarchicalException(
        List<ServiceException> serviceExceptions)
    {
        if (serviceExceptions != null && serviceExceptions.Count > 0)
        {
            DataEstateHealthErrorInfoModel parentError =
                this.GetUserFacingException(serviceExceptions.First());
            var errorDetails = new List<DataEstateHealthErrorInfoModel>();

            for (int index = 1; index < serviceExceptions.Count; index++)
            {
                ServiceException detailException = serviceExceptions[index];
                errorDetails.Add(this.GetUserFacingException(detailException));
            }

            parentError.Details = errorDetails.ToArray();

            return parentError;
        }

        return null;
    }

    /// <inheritdoc />
    public bool IsClientError(ServiceException serviceException)
    {
        HttpStatusCode userFacingExceptionStatusCode = this.GetUserFacingStatusCode(serviceException);

        return (int)userFacingExceptionStatusCode >= 400 && (int)userFacingExceptionStatusCode < 500;
    }

    /// <inheritdoc />
    public bool IsTransientError(ServiceException serviceException)
    {
        return serviceException.ServiceError.Category == ErrorCategory.TransientStateError;
    }

    public string JoinErrorMessages(IList<ServiceException> serviceExceptions)
    {
        if (serviceExceptions == null)
        {
            return string.Empty;
        }

        return string.Join(
            ";",
            serviceExceptions.Select(
                serviceException => serviceException.Message));
    }

    private static string TranslateToUserFacingCode(ServiceErrorCode errorCode)
    {
        return TranslateToUserFacingCode(errorCode.Code);
    }

    private static string TranslateToUserFacingCode(int errorCode)
    {
        return errorCode.ToString(CultureInfo.InvariantCulture);
    }
}
