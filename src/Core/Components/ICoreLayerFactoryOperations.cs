﻿// -----------------------------------------------------------
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
    /// Creates an instance of BusinessDomainCollectionComponent. 
    /// </summary>
    /// <returns>An <see cref="IBusinessDomainCollectionComponent"/>.</returns>
    public IBusinessDomainCollectionComponent CreateBusinessDomainCollectionComponent(
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

    /// <summary>
    /// Creates an instance of HealthActionCollectionComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthActionCollectionComponent"/>.</returns>
    public IHealthActionCollectionComponent CreateHealthActionCollectionComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of HealthScoreCollectionComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthScoreCollectionComponent"/>.</returns>
    public IHealthScoreCollectionComponent CreateHealthScoreCollectionComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of HealthControlCollectionComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthControlCollectionComponent"/>.</returns>
    public IHealthControlCollectionComponent CreateHealthControlCollectionComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of HealthTrendComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthTrendComponent"/>.</returns>
    public IHealthTrendComponent CreateHealthTrendComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of RefreshHistoryComponent. 
    /// </summary>
    /// <returns>An <see cref="IRefreshHistoryComponent"/>.</returns>
    public IRefreshHistoryComponent CreateRefreshHistoryComponent(
        Guid tenantId,
        Guid accountId,
        Guid datasetId,
        int? top = null);

    /// <summary>
    /// Creates an instance of DataQualityScoreCollectionComponent. 
    /// </summary>
    /// <returns>An <see cref="IDataQualityScoreCollectionComponent"/>.</returns>
    public IDataQualityScoreCollectionComponent CreateDataQualityScoresCollectionComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of DataQualityScoresComponent. 
    /// </summary>
    /// <returns>An <see cref="IDataQualityScoreComponent"/>.</returns>
    public IDataQualityScoreComponent CreateDataQualityScoresComponent(
        Guid tenantId,
        Guid accountId,
        Guid domainId,
        Guid dataProductId,
        Guid dataAssetId);

    /// <summary>
    /// Creates an instance of DHControlTriggerComponent. 
    /// </summary>
    /// <returns>An <see cref="IDHWorkerServiceTriggerComponent"/>.</returns>
    public IDHWorkerServiceTriggerComponent CreateDHWorkerServiceTriggerComponent(Guid tenantId, Guid accountId);
}
