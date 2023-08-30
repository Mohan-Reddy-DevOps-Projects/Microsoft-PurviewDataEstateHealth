// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;

/// <inheritdoc />
public class RequestHeaderContextFactory : IRequestHeaderContextFactory
{
    private RequestHeaderContext requestHeaderContext;

    /// <inheritdoc />
    public void SetContext(RequestHeaderContext context)
    {
        if (this.requestHeaderContext != null || context == null)
        {
            throw new InvalidOperationException();
        }

        this.requestHeaderContext = context;
    }

    /// <inheritdoc />
    public RequestHeaderContext GetContext() => this.requestHeaderContext;
}
