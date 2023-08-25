// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Core layer factory implementation class.
/// </summary>
internal class CoreLayerFactory : ICoreLayerFactory
{
    private readonly IComponentRuntime componentRuntime;

    private readonly IComponentContextFactory contextFactory;

    /// <summary>
    /// A constructor for the <see cref="CoreLayerFactory"/> type.
    /// </summary>
    /// <param name="componentRuntime">A registry of components.</param>
    /// <param name="contextFactory">A factory that creates component contexts.</param>
    public CoreLayerFactory(
        IComponentRuntime componentRuntime,
        IComponentContextFactory contextFactory)
    {
        this.componentRuntime = componentRuntime;
        this.contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public ICoreLayerFactoryOperations Of(ServiceVersion version)
    {
        return new CoreLayerFactoryOperations(
            version,
            this.componentRuntime,
            this.contextFactory);
    }
}
