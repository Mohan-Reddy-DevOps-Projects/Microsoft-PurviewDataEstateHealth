// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Errors;

/// <summary>
/// IExceptionAdapter
/// </summary>
public interface IExceptionAdapterService
{
    /// <summary>
    /// Convert Error type
    /// </summary>
    /// <param name="serviceException"></param>
    /// <returns></returns>
    DataEstateHealthErrorInfo GetUserFacingException(ServiceException serviceException);

    /// <summary>
    /// Get User Facing error Code
    /// </summary>
    /// <param name="serviceException"></param>
    /// <returns></returns>
    HttpStatusCode GetUserFacingStatusCode(ServiceException serviceException);

    /// <summary>
    /// Return a default user facing exception
    /// </summary>
    /// <returns></returns>
    DataEstateHealthErrorInfo GetDefaultUserFacingException();

    /// <summary>
    /// Create Hierarchical purview share error info
    /// </summary>
    /// <param name="serviceExceptions"></param>
    /// <returns></returns>
    DataEstateHealthErrorInfo GetUserFacingHierarchicalException(List<ServiceException> serviceExceptions);

    /// <summary>
    /// check if it is a client exception
    /// </summary>
    /// <param name="serviceException"></param>
    /// <returns></returns>
    bool IsClientError(ServiceException serviceException);

    /// <summary>
    /// check if the exception is a transient error
    /// </summary>
    /// <param name="serviceException"></param>
    /// <returns></returns>
    bool IsTransientError(ServiceException serviceException);

    /// <summary>
    /// Join exception messages
    /// </summary>
    /// <param name="serviceExceptions"></param>
    /// <returns></returns>
    string JoinErrorMessages(IList<ServiceException> serviceExceptions);
}
