// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// The request context accessor.
/// </summary>
public interface IRequestContextAccessor
{
    /// <summary>
    /// Gets the request context.
    /// </summary>
    /// <returns></returns>
    IRequestContext GetRequestContext();

    /// <summary>
    /// Sets the request context.
    /// </summary>
    /// <param name="requestContext"></param>
    void SetRequestContext(IRequestContext requestContext);
}
