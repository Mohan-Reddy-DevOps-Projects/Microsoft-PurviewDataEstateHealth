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
}
