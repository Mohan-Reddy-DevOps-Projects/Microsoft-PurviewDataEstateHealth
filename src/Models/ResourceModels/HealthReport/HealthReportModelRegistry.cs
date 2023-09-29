// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Serves implementation classes for health report kinds.
/// </summary>
public class HealthReportModelRegistry : Registry<HealthReportModelAttribute, HealthReportKind, IHealthReportModel<HealthReportProperties>>
{
    private static readonly Lazy<HealthReportModelRegistry> instance =
        new Lazy<HealthReportModelRegistry>(() => new HealthReportModelRegistry());

    /// <summary>
    /// Prevents creating instances of the <see cref="HealthReportModelRegistry"/> class.
    /// </summary>
    private HealthReportModelRegistry() : base(att => att.HealthReportKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="HealthReportModelRegistry"/> class.
    /// </summary>
    public static HealthReportModelRegistry Instance => HealthReportModelRegistry.instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding health report model implementation for the given healthReport Kind.
    /// </summary>
    /// <param name="healthReportKind">The health report kind.</param>
    /// <param name="copyOf">An optional existing health report model to copy from.</param>
    /// <returns>The configured health report model instance.</returns>
    public IHealthReportModel<HealthReportProperties> CreateHealthReportModelFor(
        HealthReportKind healthReportKind,
        IHealthReportModel<HealthReportProperties> copyOf = null) => this.Resolve(healthReportKind, copyOf);
}
