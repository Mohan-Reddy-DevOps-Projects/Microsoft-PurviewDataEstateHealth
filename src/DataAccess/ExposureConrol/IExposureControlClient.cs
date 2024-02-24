// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.ExposureControlLibrary;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Exposure Control Client
/// </summary>
public interface IExposureControlClient
{
    /// <summary>
    /// Determines if a provided feature is enabled
    /// </summary>
    /// <param name="feature">The feature name</param>
    /// <param name="accountId">The account id</param>
    /// <param name="subscriptionId">The subscription id</param>
    /// <param name="tenantId">The tenant id</param>
    /// <returns></returns>
    bool IsFeatureEnabled(string feature, string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Gets an allow list
    /// </summary>
    /// <param name="allowList">The allow list</param>
    /// <param name="accountId">The accountId</param>
    /// <param name="subscriptionId">The subscription id</param>
    /// <param name="tenantId">The tenant id</param>
    /// <returns></returns>
    string GetAllowList(string allowList, string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Gets dictionary value.
    /// </summary>
    /// <param name="dictionaryName">The dictionary name.</param>
    /// <returns>The dictionary value.</returns>
    Dictionary<string, string> GetDictionaryValue(string dictionaryName);

    /// <summary>
    /// Gets the reference to the exposure control
    /// </summary>
    IExposureControl GetExposureControl();

    /// <summary>
    /// Initializes the provider.
    /// </summary>
    Task Initialize();
}
