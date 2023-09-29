// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;

/// <summary>
/// Defines an attribute which is used to identify health report implementation classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class HealthReportModelAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthReportModelAttribute"/> class.
    /// </summary>
    /// <param name="healthReportKind">The share kind the class handles.</param>
    public HealthReportModelAttribute(HealthReportKind healthReportKind)
    {
        this.HealthReportKind = healthReportKind;
    }

    /// <summary>
    /// Gets the health report kind the class handles.
    /// </summary>
    public HealthReportKind HealthReportKind { get; }
}
