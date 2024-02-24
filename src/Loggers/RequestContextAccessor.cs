// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Loggers;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class RequestContextAccessor : IRequestContextAccessor
{
    private static readonly AsyncLocal<IRequestContext> asyncLocalRequestContext = new();
    private const string HttpContextKey = "PurviewRequestContext";
    private readonly IHttpContextAccessor httpContextAccessor;

    public RequestContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public IRequestContext GetRequestContext()
    {
        if (asyncLocalRequestContext.Value != null)
        {
            return asyncLocalRequestContext.Value;
        }

        if (this.httpContextAccessor?.HttpContext?.Items != null &&
            this.httpContextAccessor.HttpContext.Items.TryGetValue(HttpContextKey, out var item) &&
            item is IRequestHeaderContext typedRequestContext)
        {
            return typedRequestContext;
        }

        var requestContext = new RequestHeaderContext(this.httpContextAccessor);

        this.SetRequestContext(requestContext);

        return requestContext;
    }

    public void SetRequestContext(IRequestContext requestContext)
    {
        if (this.httpContextAccessor?.HttpContext?.Items != null)
        {
            this.httpContextAccessor.HttpContext.Items[HttpContextKey] = requestContext;
        }

        asyncLocalRequestContext.Value = requestContext;
    }
}
