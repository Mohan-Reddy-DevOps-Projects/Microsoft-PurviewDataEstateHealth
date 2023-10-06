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
}
