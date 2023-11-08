// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities;

using System;

/// <summary>
/// Spark utilities class for shared static methods.
/// </summary>
public static class SparkUtilities
{
    /// <summary>
    /// Constructs the spark pool name string.
    /// </summary>
    /// <param name="prefix">The storage account name prefix.</param>
    /// <returns>The storage account name.</returns>
    public static string ConstructSparkPoolName(string prefix)
    {
        const int maxLength = 15;
        const int maxPrefixLength = maxLength - 4;
        const int maxRandomStringLength = 7;
        int prefixLength = Math.Min(prefix.Length, maxPrefixLength);
        prefix = prefix?[..prefixLength];
        int randomStringLength = Math.Min(maxLength - prefix.Length, maxRandomStringLength);

        // The name can contain only lowercase letters and numbers.
        // Also, length must be between 1 and 15 characters.
        string name = $"{prefix}{RandomStringBuilder.Generate(randomStringLength)}".ToLowerInvariant();

        return name[..Math.Min(name.Length, maxLength)];
    }
}
