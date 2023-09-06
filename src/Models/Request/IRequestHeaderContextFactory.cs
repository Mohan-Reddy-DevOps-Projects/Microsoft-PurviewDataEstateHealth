// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;

/// <summary>
/// Factory to inject scoped service into worker role.
/// </summary>
public interface IRequestHeaderContextFactory
{
    /// <summary>
    /// Set the context
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="InvalidOperationException"></exception>
    void SetContext(RequestHeaderContext context);

    /// <summary>
    /// Get the context
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    RequestHeaderContext GetContext();
}
