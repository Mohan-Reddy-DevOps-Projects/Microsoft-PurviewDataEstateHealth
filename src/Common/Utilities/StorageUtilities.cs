// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Common.Utilities;

using System;
using System.Text.RegularExpressions;

/// <summary>
/// Storage utilties class for shared static methods.
/// </summary>
public static partial class StorageUtilities
{
    private static readonly Regex EndpointMatch = new(@"^(https?\:\/\/)?\w+\.blob\.([\w\.]+)\/?", RegexOptions.Compiled);
    private static readonly Regex EndpointMatchDnsEnabled = new Regex(@"^(https?\:\/\/)?\w+\.(\w+)\.blob\.([\w\.]+)\/?", RegexOptions.Compiled);

    /// <summary>
    /// Constructs the storage account name string.
    /// </summary>
    /// <param name="prefix">The storage account name prefix.</param>
    /// <returns>The storage account name.</returns>
    public static string ConstructStorageAccountName(string prefix)
    {
        const int maxStorageAccountNameLength = 24;
        const int maxStorageAccountNamePrefixLength = maxStorageAccountNameLength - 4;
        const int maxRandomStringLength = 7;
        int prefixLength = Math.Min(prefix.Length, maxStorageAccountNamePrefixLength);
        prefix = prefix?[..prefixLength];
        int randomStringLength = Math.Min(maxStorageAccountNameLength - prefix.Length, maxRandomStringLength);

        // The storage account name can contain only lowercase letters and numbers.
        // Also, length must be between 3 and 24 characters.
        string storageAccountName = $"{prefix}{RandomStringBuilder.Generate(randomStringLength)}".ToLowerInvariant();

        return storageAccountName[..Math.Min(storageAccountName.Length, maxStorageAccountNameLength)];
    }

    /// <summary>
    /// Extracts the DNS Zone from the storage primary endpoint.
    /// </summary>
    /// <param name="endpoint">The storage primary endpoint.</param>
    /// <returns>the dns zone.</returns>
    public static string GetStorageDnsZoneFromEndpoint(Uri endpoint)
    {
        return EndpointMatchDnsEnabled.Replace(endpoint.ToString(), "$2");
    }

    /// <summary>
    /// Extracts the storage endpoint suffix from the storage primary endpoint.
    /// </summary>
    /// <param name="endpoint">The storage primary endpoint.</param>
    /// <param name="isDnsEnabled">if the storage account is in a partition DNS enabled subscription.</param>
    /// <returns>the endpoint suffix of the storage account endpoint.</returns>
    public static string GetStorageEndpointSuffix(Uri endpoint, bool isDnsEnabled)
    {
        if (isDnsEnabled)
        {
            return EndpointMatchDnsEnabled.Replace(endpoint.ToString(), "$3");
        }
        else
        {
            return EndpointMatch.Replace(endpoint.ToString(), "$2");
        }
    }
}
