// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Registry for queries
/// </summary>
public class ServerlessQueryRegistry : Registry<ServerlessQueryAttribute, Type, IServerlessQueryRequest<BaseRecord, BaseEntity>>
{
    private static readonly Lazy<ServerlessQueryRegistry> instance =
            new Lazy<ServerlessQueryRegistry>(() => new ServerlessQueryRegistry());

    /// <summary>
    /// Prevents creating instances of the class.
    /// </summary>
    private ServerlessQueryRegistry() : base(att => att.RecordKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the class.
    /// </summary>
    public static ServerlessQueryRegistry Instance => instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding IServerlessQueryRequest implementation for the given entity Kind.
    /// </summary>
    public IServerlessQueryRequest<BaseRecord, BaseEntity> CreateQueryFor(Type recordType,
        IServerlessQueryRequest<BaseRecord, BaseEntity> copyOf = null) => this.Resolve(recordType, copyOf);
}

