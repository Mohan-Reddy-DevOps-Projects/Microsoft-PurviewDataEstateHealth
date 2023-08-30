// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;

/// <summary>
/// Extension methods to sanitize values to be logged
/// </summary>
public static class IfxExtensions
{
    /// <summary>
    /// Add a default value when null for string value to be logged.
    /// </summary>
    /// <param name="value">The input value to be sanitized</param>
    /// <returns>The sanitized string value.</returns>
    public static string ConvertDefault(this string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }
}
