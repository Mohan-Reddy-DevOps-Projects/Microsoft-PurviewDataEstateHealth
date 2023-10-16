// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines an attribute which is used to identify health score entity implementation classes and bind
/// them to the score kinds they belong to.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal class HealthScoreEntityAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthScoreEntityAttribute"/> class.
    /// </summary>
    /// <param name="scoreKind">The score kind of the health score the class handles.</param>
    public HealthScoreEntityAttribute(HealthScoreKind scoreKind)
    {
        this.ScoreKind = scoreKind;
    }

    /// <summary>
    /// Gets the score kind of the health score the class handles.
    /// </summary>
    public HealthScoreKind ScoreKind { get; }
}
