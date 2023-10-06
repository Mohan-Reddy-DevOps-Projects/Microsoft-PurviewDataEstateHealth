// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Core layer factory operations.
/// </summary>
public interface ICoreLayerFactoryOperations
{
    /// <summary>
    /// Creates an instance of DataHealthEstateSummaryComponent. 
    /// </summary>
    /// <returns>An <see cref="IDataEstateHealthSummaryComponent"/>.</returns>
    public IDataEstateHealthSummaryComponent CreateDataEstateHealthSummaryComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of HealthReportCollectionComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthReportCollectionComponent"/>.</returns>
    public IHealthReportCollectionComponent CreateHealthReportCollectionComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of HealthReportComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthReportComponent"/>.</returns>
    public IHealthReportComponent CreateHealthReportComponent(
        Guid tenantId,
        Guid accountId,
        Guid reportId);

    /// <summary>
    /// Creates an instance of TokenComponent. 
    /// </summary>
    /// <returns>An <see cref="ITokenComponent"/>.</returns>
    public ITokenComponent CreateTokenComponent(
        Guid tenantId,
        Guid accountId,
        string owner);

    /// <summary>
    /// Creates an instance of PartnerNotificationComponent. 
    /// </summary>
    /// <returns>An <see cref="IPartnerNotificationComponent"/>.</returns>
    public IPartnerNotificationComponent CreatePartnerNotificationComponent(
        Guid tenantId,
        Guid accountId);
}
