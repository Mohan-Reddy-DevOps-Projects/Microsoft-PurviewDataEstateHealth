// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Registries;

internal class HealthScoreEntityRegistry : ImplementationRegistry<HealthScoreEntityAttribute, HealthScoreKind, HealthScoreEntity>
{
    private static readonly Lazy<HealthScoreEntityRegistry> instance =
        new Lazy<HealthScoreEntityRegistry>(() => new HealthScoreEntityRegistry());

    /// <summary>
    /// Prevents creating instances of the <see cref="HealthScoreEntityRegistry"/> class.
    /// </summary>
    private HealthScoreEntityRegistry()
        : base(att => att.ScoreKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="HealthScoreEntityRegistry"/> class.
    /// </summary>
    public static HealthScoreEntityRegistry Instance => HealthScoreEntityRegistry.instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding Health score model implementation for the given score kind.
    /// </summary>
    /// <param name="scoreKind">The score kind.</param>
    /// <param name="copyOf">An optional existing Health Score entity properties to copy from.</param>
    /// <returns>The configured Health Score entity instance.</returns>
    public HealthScoreEntity CreateHealthScoreEntity(HealthScoreKind scoreKind, HealthScoreEntity copyOf = null) =>
        this.Resolve(scoreKind, copyOf);
}
