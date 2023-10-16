// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Serves implementation classes for health score kinds.
/// </summary>
public class HealthScoreModelRegistry : Registry<HealthScoreModelAttribute, HealthScoreKind, IHealthScoreModel<HealthScoreProperties>>
{
    private static readonly Lazy<HealthScoreModelRegistry> instance =
        new Lazy<HealthScoreModelRegistry>(() => new HealthScoreModelRegistry());

    /// <summary>
    /// Prevents creating instances of the <see cref="HealthScoreModelRegistry"/> class.
    /// </summary>
    private HealthScoreModelRegistry() : base(att => att.HealthScoreKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="HealthScoreModelRegistry"/> class.
    /// </summary>
    public static HealthScoreModelRegistry Instance => HealthScoreModelRegistry.instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding health socre model implementation for the given health score Kind.
    /// </summary>
    /// <param name="healthScoreKind">The health score kind.</param>
    /// <param name="copyOf">An optional existing health report model to copy from.</param>
    /// <returns>The configured health score model instance.</returns>
    public IHealthScoreModel<HealthScoreProperties> CreateHealthScoreModelFor(
        HealthScoreKind healthScoreKind,
        IHealthScoreModel<HealthScoreProperties> copyOf = null) => this.Resolve(healthScoreKind, copyOf);
}
