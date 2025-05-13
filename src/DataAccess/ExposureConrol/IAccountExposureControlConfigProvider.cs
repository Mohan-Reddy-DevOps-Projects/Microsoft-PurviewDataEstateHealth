// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Purview.DataGovernance.Reporting.Models;

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
    public bool IsDQPBIUpgradeEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if schedule queue for Data Governance Health is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthScheduleQueueEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if SQL setup running enabled before PBI refresh. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthRunSetupSQLEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DG health tips is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthTipsEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DG health DQ report is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovHealthDQReportEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DG usage settings UX is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovBYOCEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DG Billing Event is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDataGovBillingEventEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DEH Billing Event is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDEHBillingEventEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DQ Billing Event is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDQBillingEventEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DEH Billing Domain Split is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDEHBillingSplitEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DQ Billing Domain Split is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDQBillingSplitEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DEH data cleanup is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDEHDataCleanup(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DEH Auto Generate actionProperties enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDEHEnableAutoGenerateRulesDescriptiveFieldsValue(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DEH Business OKRs Alignment enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDEHBusinessOKRsAlignmentEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Determines if DEH Critical data identification is enabled. By default this is false.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="subscriptionId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public bool IsDEHCriticalDataIdentificationEnabled(string accountId, string subscriptionId, string tenantId);

    /// <summary>
    /// Retrieve the Spark Job configurations.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, SparkPoolECConfig> GetDGSparkJobConfig();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, CapacityModel> GetPBICapacities();


}
