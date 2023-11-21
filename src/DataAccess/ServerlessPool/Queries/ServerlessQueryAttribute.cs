// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Serverless query attribute class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServerlessQueryAttribute : Attribute
{
    /// <summary>
    /// Constructor
    /// </summary>
    public ServerlessQueryAttribute(Type recordKind)
    {
        this.RecordKind = recordKind;
    }

    /// <summary>
    /// Entity kind.
    /// </summary>
    public Type RecordKind  { get; }
}
