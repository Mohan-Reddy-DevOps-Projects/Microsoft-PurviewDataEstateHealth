// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Data column attribute class.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
internal class DataColumnAttribute : Attribute
{
    public string Name { get; }

    public DataColumnAttribute(string name)
    {
        this.Name = name;
    }
}
