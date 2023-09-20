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
}
