// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;

internal interface IComponentContextFactory
{
    /// <summary>
    /// Creates an <see cref="IDataEstateHealthSummaryComponent"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IDataEstateHealthSummaryContext CreateDataEstateHealthSummaryContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IHealthReportListContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IHealthReportListContext CreateHealthReportListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IHealthReportContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public IHealthReportContext CreateHealthReportContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        Guid reportId);

    /// <summary>
    /// Creates an <see cref="ITokenContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public ITokenContext CreateTokenContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        string owner);

    /// <summary>
    /// Creates an <see cref="IPartnerNotificationComponent"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IPartnerNotificationContext CreatePartnerNotificationComponent(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IHealthActionListContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IHealthActionListContext CreateHealthActionListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IHealthScoreListContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IHealthScoreListContext CreateHealthScoreListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IBusinessDomainListContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IBusinessDomainListContext CreateBusinessDomainListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IHealthControlListContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IHealthControlListContext CreateHealthControlListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IHealthTrendContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IHealthTrendContext CreateHealthTrendContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IRefreshHistoryContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <param name="datasetId"></param>
    /// <param name="top"></param>
    /// <returns></returns>
    public IRefreshHistoryContext CreateRefreshHistoryContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        Guid datasetId,
        int? top = null);

    /// <summary>
    /// Creates an <see cref="IDataQualityListContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public IDataQualityListContext CreateDataQualityListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId);

    /// <summary>
    /// Creates an <see cref="IDataQualityContext"/>
    /// </summary>
    /// <param name="version"></param>
    /// <param name="location"></param>
    /// <param name="accountId"></param>
    /// <param name="tenantId"></param>
    /// <param name="domainId"></param>
    /// <param name="dataProductId"></param>
    /// <param name="dataAssetId"></param>
    /// <returns></returns>
    public IDataQualityContext CreateDataQualityContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        Guid domainId,
        Guid dataProductId,
        Guid dataAssetId);
}
