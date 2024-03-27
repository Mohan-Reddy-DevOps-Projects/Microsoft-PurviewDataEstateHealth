// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;

internal class ComponentContextFactory : IComponentContextFactory
{
    private readonly EnvironmentConfiguration environmentConfiguration;

    public ComponentContextFactory(IOptions<EnvironmentConfiguration> environmentConfiguration)
    {
        this.environmentConfiguration = environmentConfiguration.Value;
    }

    /// <inheritdoc />
    public IDataEstateHealthSummaryContext CreateDataEstateHealthSummaryContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new DataEstateHealthSummaryContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IHealthReportListContext CreateHealthReportListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new HealthReportListContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IHealthReportContext CreateHealthReportContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        Guid reportId)
    {
        return new HealthReportContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId,
            ReportId = reportId
        };
    }

    /// <inheritdoc />
    public ITokenContext CreateTokenContext(
        ServiceVersion version,
        string location,
        Guid tenantId,
        Guid accountId,
        string owner)
    {
        return new TokenContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId,
            Owner = owner
        };
    }

    /// <inheritdoc />
    public IPartnerNotificationContext CreatePartnerNotificationComponent(
        ServiceVersion version,
        string location,
        Guid tenantId,
        Guid accountId)
    {
        return new PartnerNotificationContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IHealthActionListContext CreateHealthActionListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new HealthActionListContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IBusinessDomainListContext CreateBusinessDomainListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new BusinessDomainListContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IHealthScoreListContext CreateHealthScoreListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new HealthScoreListContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IHealthControlListContext CreateHealthControlListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new HealthControlListContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IHealthTrendContext CreateHealthTrendContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new HealthTrendContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IRefreshHistoryContext CreateRefreshHistoryContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        Guid datasetId,
        int? top = null)
    {
        return new RefreshHistoryContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId,
            DatasetId = datasetId,
            Top = top
        };
    }

    /// <inheritdoc />
    public IDataQualityListContext CreateDataQualityListContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId)
    {
        return new DataQualityListContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    /// <inheritdoc />
    public IDataQualityContext CreateDataQualityContext(
        ServiceVersion version,
        string location,
        Guid accountId,
        Guid tenantId,
        Guid domainId,
        Guid dataProductId,
        Guid dataAssetId)
    {
        return new DataQualityContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId,
            BusinessDomainId = domainId,
            DataProductId = dataProductId,
            DataAssetId = dataAssetId
        };
    }


    /// <inheritdoc />
    public IDHControlTriggerContext CreateDHControlTriggerContext(
        ServiceVersion version,
        string location,
        Guid tenantId,
        Guid accountId)
    {
        return new DHControlTriggerContext
        {
            Version = version,
            Location = this.LocationOf(location),
            TenantId = tenantId,
            AccountId = accountId
        };
    }

    internal string LocationOf(string location)
    {
        return string.IsNullOrWhiteSpace(location) ? this.environmentConfiguration.Location : location;
    }
}
