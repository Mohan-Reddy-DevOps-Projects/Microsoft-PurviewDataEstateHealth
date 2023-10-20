// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Components;

/// <inheritdoc />
internal class CoreLayerFactoryOperations : ICoreLayerFactoryOperations
{
    private readonly ServiceVersion version;

    private readonly IComponentRuntime componentRuntime;

    private readonly IComponentContextFactory contextFactory;

    /// <summary>
    /// Constructor for the <see cref="CoreLayerFactoryOperations"/> type.
    /// </summary>
    /// <param name="version">The version of the service.</param>
    /// <param name="componentRuntime">A registry of components.</param>
    /// <param name="contextFactory">A factory that creates component contexts.</param>
    public CoreLayerFactoryOperations(
        ServiceVersion version,
        IComponentRuntime componentRuntime,
        IComponentContextFactory contextFactory)
    {
        this.version = version;
        this.componentRuntime = componentRuntime;
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public IDataEstateHealthSummaryComponent CreateDataEstateHealthSummaryComponent(
        Guid tenantId,
        Guid accountId)
    {
        return this.componentRuntime.ResolveLatest<IDataEstateHealthSummaryComponent, IDataEstateHealthSummaryContext>(
            this.contextFactory.CreateDataEstateHealthSummaryContext(
                this.version,
                null,
                accountId,
                tenantId));
    }

    /// <inheritdoc />
    public IBusinessDomainCollectionComponent CreateBusinessDomainCollectionComponent(
        Guid tenantId,
        Guid accountId)
    {
        return this.componentRuntime.ResolveLatest<IBusinessDomainCollectionComponent, IBusinessDomainListContext>(
            this.contextFactory.CreateBusinessDomainListContext(
                this.version,
                null,
                accountId,
                tenantId));
    }

    /// <inheritdoc />
    public IHealthReportCollectionComponent CreateHealthReportCollectionComponent(
        Guid tenantId,
        Guid accountId)
    {
        return this.componentRuntime.Resolve<IHealthReportCollectionComponent, IHealthReportListContext>(
            this.contextFactory.CreateHealthReportListContext(
                this.version,
                null,
                accountId,
                tenantId),
            this.version.Numeric);
    }

    /// <inheritdoc />
    public IHealthReportComponent CreateHealthReportComponent(
        Guid tenantId,
        Guid accountId,
        Guid reportId)
    {
        return this.componentRuntime.Resolve<IHealthReportComponent, IHealthReportContext>(
            this.contextFactory.CreateHealthReportContext(
                this.version,
                null,
                accountId,
                tenantId,
                reportId),
            this.version.Numeric);
    }

    /// <inheritdoc />
    public ITokenComponent CreateTokenComponent(Guid tenantId, Guid accountId, string owner)
    {
        return this.componentRuntime.Resolve<ITokenComponent, ITokenContext>(
            this.contextFactory.CreateTokenContext(
                this.version,
                null,
                tenantId,
                accountId,
                owner),
            this.version.Numeric);
    }

    /// <inheritdoc />
    public IPartnerNotificationComponent CreatePartnerNotificationComponent(Guid tenantId, Guid accountId)
    {
        return this.componentRuntime.Resolve<IPartnerNotificationComponent, IPartnerNotificationContext>(
            this.contextFactory.CreatePartnerNotificationComponent(
                this.version,
                null,
                tenantId,
                accountId),
            this.version.Numeric);
    }

    /// <inheritdoc />
    public IHealthActionCollectionComponent CreateHealthActionCollectionComponent(
        Guid tenantId,
        Guid accountId)
    {
        return this.componentRuntime.Resolve<IHealthActionCollectionComponent, IHealthActionListContext>(
            this.contextFactory.CreateHealthActionListContext(
                this.version,
                null,
                accountId,
                tenantId),
            this.version.Numeric);
    }

    /// <inheritdoc />
    public IHealthScoreCollectionComponent CreateHealthScoreCollectionComponent(
        Guid tenantId,
        Guid accountId)
    {
        return this.componentRuntime.Resolve<IHealthScoreCollectionComponent, IHealthScoreListContext>(
            this.contextFactory.CreateHealthScoreListContext(
                this.version,
                null,
                accountId,
                tenantId),
            this.version.Numeric);
    }

    /// <inheritdoc />
    public IHealthControlCollectionComponent CreateHealthControlCollectionComponent(
        Guid tenantId,
        Guid accountId)
    {
        return this.componentRuntime.Resolve<IHealthControlCollectionComponent, IHealthControlListContext>(
            this.contextFactory.CreateHealthControlListContext(
                this.version,
                null,
                accountId,
                tenantId),
            this.version.Numeric);
    }
}
