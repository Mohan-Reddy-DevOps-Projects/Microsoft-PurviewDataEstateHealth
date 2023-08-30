// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Extensions methods for request header processing.
/// </summary>
public static class RequestExtensions
{
    /// <summary>
    /// Gets the value for the given header when present or use a default value.
    /// </summary>
    /// <param name="headers">The headers to look into.</param>
    /// <param name="name">The header name to look for.</param>
    /// <param name="defaultValue">The default value to use.</param>
    /// <returns></returns>
    public static string GetFirstOrDefault(this IHeaderDictionary headers, string name, string defaultValue = null)
    {
        if (headers == null || !headers.ContainsKey(name))
        {
            return defaultValue;
        }

        return headers[name];
    }

    /// <summary>
    /// Gets the value of query string parameter of a http request.
    /// </summary>
    /// <param name="httpRequest">The http request to get the query string value.</param>
    /// <param name="name">Name of the query string to look for.</param>
    /// <param name="defaultValue">Default value to use when not found.</param>
    /// <returns></returns>
    public static string GetFirstOrDefaultQuery(
        this HttpRequest httpRequest,
        string name,
        string defaultValue = null)
    {
        return httpRequest.Query.TryGetValue(name, out StringValues queryValues)
            ? queryValues.FirstOrDefault()
            : null;
    }

    /// <summary>
    /// Gets the guid value of the given header when present or empty guid otherwise.
    /// </summary>
    /// <param name="headers">The headers to look into.</param>
    /// <param name="name">The header name to look for.</param>
    /// <returns></returns>
    public static Guid GetFirstOrDefaultGuid(this IHeaderDictionary headers, string name)
    {
        return Guid.TryParse(headers.GetFirstOrDefault(name), out Guid result) ? result : Guid.Empty;
    }
}
