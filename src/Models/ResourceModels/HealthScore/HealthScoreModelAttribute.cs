// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using System;

/// <summary>
/// Defines an attribute which is used to identify health score implementation classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class HealthScoreModelAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthScoreModelAttribute"/> class.
    /// </summary>
    /// <param name="healthScoreKind">The share kind the class handles.</param>
    public HealthScoreModelAttribute(HealthScoreKind healthScoreKind)
    {
        this.HealthScoreKind = healthScoreKind;
    }

    /// <summary>
    /// Gets the health score kind the class handles.
    /// </summary>
    public HealthScoreKind HealthScoreKind { get; }
}
