// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines an attribute which is used to identify health control implementation classes.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class HealthControlModelAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthControlModelAttribute"/> class.
    /// </summary>
    /// <param name="healthControlKind">The share kind the class handles.</param>
    public HealthControlModelAttribute(HealthControlKind healthControlKind)
    {
        this.HealthControlKind = healthControlKind;
    }

    /// <summary>
    /// Gets the health control kind the class handles.
    /// </summary>
    public HealthControlKind HealthControlKind { get; }
}
