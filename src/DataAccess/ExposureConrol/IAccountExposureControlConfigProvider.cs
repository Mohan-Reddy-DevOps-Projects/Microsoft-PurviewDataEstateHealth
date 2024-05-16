// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
/// <summary>
/// Expose Exposure Control APIs
/// </summary>
public interface IAccountExposureControlConfigProvider
{
    /// <summary>
    /// Determines if provisioning for Data Quality is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId">The accountId</param>
    /// <param name="subscriptionId">The subscription id</param>
    /// <param name="tenantId">The tenant id</param>
    /// <returns></returns>
    public bool IsDataQualityProvisioningEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if upgrading PBI reports for Data Governance Health is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthPBIUpgradeEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if schedule queue for Data Governance Health is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthScheduleQueueEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if new DG health is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDGDataHealthEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DG health tips is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthTipsEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if provisioning for Data Governance Health is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthProvisioningEnabled(string accountId, string subscriptionId, string tenantId);
}
