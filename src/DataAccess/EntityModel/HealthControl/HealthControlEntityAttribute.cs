// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines an attribute which is used to identify health control entity implementation classes and bind
/// them to the control kinds they belong to.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class HealthControlEntityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthControlEntityAttribute"/> class.
    /// </summary>
    /// <param name="controlKind">The control kind of the health control the class handles.</param>
    public HealthControlEntityAttribute(HealthControlKind controlKind)
    {
        this.ControlKind = controlKind;
    }

    /// <summary>
    /// Gets the control kind of the health control the class handles.
    /// </summary>
    public HealthControlKind ControlKind { get; }
}
