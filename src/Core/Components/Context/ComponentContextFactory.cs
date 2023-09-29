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
        Guid tenantId,
        Guid accountId)
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
        Guid tenantId,
        Guid accountId)
    {
        return new HealthReportListContext
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
