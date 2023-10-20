// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Registries;

internal class HealthControlEntityRegistry : ImplementationRegistry<HealthControlEntityAttribute, HealthControlKind, HealthControlEntity>
{
    private static readonly Lazy<HealthControlEntityRegistry> instance =
        new Lazy<HealthControlEntityRegistry>(() => new HealthControlEntityRegistry());

    /// <summary>
    /// Prevents creating instances of the <see cref="HealthControlEntityRegistry"/> class.
    /// </summary>
    private HealthControlEntityRegistry()
        : base(att => att.ControlKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="HealthControlEntityRegistry"/> class.
    /// </summary>
    public static HealthControlEntityRegistry Instance => HealthControlEntityRegistry.instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding Health control model implementation for the given control kind.
    /// </summary>
    /// <param name="controlKind">The Control kind.</param>
    /// <param name="copyOf">An optional existing Health control entity properties to copy from.</param>
    /// <returns>The configured Health Score entity instance.</returns>
    public HealthControlEntity CreateHealthControlEntity(HealthControlKind controlKind, HealthControlEntity copyOf = null) =>
        this.Resolve(controlKind, copyOf);
}
