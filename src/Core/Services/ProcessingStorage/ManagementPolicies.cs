// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using global::Azure.ResourceManager.Storage;
using global::Azure.ResourceManager.Storage.Models;

internal static class ManagementPolicies
{
    public static StorageAccountManagementPolicyData GetStorageManagementPolicyRules()
    {
        return new StorageAccountManagementPolicyData()
        {
            Rules = new List<ManagementPolicyRule>
            {
                GetStorageManagementPolicyRule(Keep30dStoragePolicyName, Keep30dStoragePolicyPrefixMatch, Keep30dStoragePolicyTimeToLiveDays),
                GetStorageManagementPolicyRule(Keep60dStoragePolicyName, Keep60dStoragePolicyPrefixMatch, Keep60dStoragePolicyTimeToLiveDays),
                GetStorageManagementPolicyRule(Keep90dStoragePolicyName, Keep90dStoragePolicyPrefixMatch, Keep90dStoragePolicyTimeToLiveDays),
            }
        };        
    }

    public static ManagementPolicyRule GetStorageManagementPolicyRule(
        string name,
        IList<string> prefixMatches,
        float timeToLiveDays)
    {
        ManagementPolicyAction actions = new()
        {
            BaseBlob = new ManagementPolicyBaseBlob
            {
                Delete = new DateAfterModification
                {
                    DaysAfterModificationGreaterThan = timeToLiveDays,
                },
            },
        };

        // Only delete is supported for appendBlob
        ManagementPolicyFilter filter = new ManagementPolicyFilter(new List<string> { "blockBlob", "appendBlob" });

        foreach (string prefixMatch in prefixMatches)
        {
            filter.PrefixMatch.Add(prefixMatch);
        }

        ManagementPolicyDefinition definition = new(actions)
        {
            Filters = filter
        };
        return new ManagementPolicyRule(name, ManagementPolicyRuleType.Lifecycle, definition)
        {
            IsEnabled = true,
        };
    }

    /// <summary>
    /// The keep30d storage policy name.
    /// </summary>
    public const string Keep30dStoragePolicyName = "CleanupTransientBlobs30d";

    /// <summary>
    /// The keep30d storage policy TTL of 30 days.
    /// </summary>
    public const float Keep30dStoragePolicyTimeToLiveDays = 30;

    /// <summary>
    /// The keep30d storage policy blob prefix match.
    /// </summary>
    public static IList<string> Keep30dStoragePolicyPrefixMatch = new List<string>() { "keep30d" };

    /// <summary>
    /// The keep60d storage policy name.
    /// </summary>
    public const string Keep60dStoragePolicyName = "CleanupTransientBlobs60d";

    /// <summary>
    /// The keep60d storage policy TTL of 60 days.
    /// </summary>
    public const float Keep60dStoragePolicyTimeToLiveDays = 60;

    /// <summary>
    /// The keep60d storage policy blob prefix match.
    /// </summary>
    public static IList<string> Keep60dStoragePolicyPrefixMatch = new List<string>() { "keep60d" };

    /// <summary>
    /// The keep90d storage policy name.
    /// </summary>
    public const string Keep90dStoragePolicyName = "CleanupTransientBlobs90d";

    /// <summary>
    /// The keep90d storage policy TTL of 90 days.
    /// </summary>
    public const float Keep90dStoragePolicyTimeToLiveDays = 90;

    /// <summary>
    /// The keep90d storage policy blob prefix match.
    /// </summary>
    public static IList<string> Keep90dStoragePolicyPrefixMatch = new List<string>() { "keep90d" };
}
