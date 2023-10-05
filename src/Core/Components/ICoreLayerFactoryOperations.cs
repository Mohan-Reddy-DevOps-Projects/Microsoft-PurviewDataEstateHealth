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
    /// Creates an instance of HealthReportComponent. 
    /// </summary>
    /// <returns>An <see cref="IHealthReportCollectionComponent"/>.</returns>
    public IHealthReportCollectionComponent CreateHealthReportCollectionComponent(
        Guid tenantId,
        Guid accountId);

    /// <summary>
    /// Creates an instance of TokenComponent. 
    /// </summary>
    /// <returns>An <see cref="ITokenComponent"/>.</returns>
    public ITokenComponent CreateTokenComponent(
        Guid tenantId,
        Guid accountId,
        string owner);
}
