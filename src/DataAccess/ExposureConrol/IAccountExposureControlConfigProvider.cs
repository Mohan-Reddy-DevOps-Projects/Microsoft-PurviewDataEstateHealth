// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Expose Exposure Control APIs
/// </summary>
public interface IAccountExposureControlConfigProvider
{
    /// <summary>
    /// Determines if provisioning for Data Governance is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId">The accountId</param>
    /// <param name="subscriptionId">The subscription id</param>
    /// <param name="tenantId">The tenant id</param>
    /// <returns></returns>
    public bool IsDataGovProvisioningEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Retrieve the capacities that are available.
    /// </summary>
    /// <returns></returns>
    Dictionary<string, Capacity> GetPBICapacities();
}
