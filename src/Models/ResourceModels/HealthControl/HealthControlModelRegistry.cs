// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Serves implementation classes for health control kinds.
/// </summary>
public class HealthControlModelRegistry : Registry<HealthControlModelAttribute, HealthControlKind, IHealthControlModel<HealthControlProperties>>
{
    private static readonly Lazy<HealthControlModelRegistry> instance =
        new Lazy<HealthControlModelRegistry>(() => new HealthControlModelRegistry());

    /// <summary>
    /// Prevents creating instances of the <see cref="HealthControlModelRegistry"/> class.
    /// </summary>
    private HealthControlModelRegistry() : base(att => att.HealthControlKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="HealthControlModelRegistry"/> class.
    /// </summary>
    public static HealthControlModelRegistry Instance => HealthControlModelRegistry.instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding health control model implementation for the given health control Kind.
    /// </summary>
    /// <param name="healthControlKind">The health control kind.</param>
    /// <param name="copyOf">An optional existing health control model to copy from.</param>
    /// <returns>The configured health control model instance.</returns>
    public IHealthControlModel<HealthControlProperties> CreateHealthControlModelFor(
        HealthControlKind healthControlKind,
        IHealthControlModel<HealthControlProperties> copyOf = null) => this.Resolve(healthControlKind, copyOf);
}
