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
    /// Prevents creating instances of the <see cref="ServerlessQueryRegistry"/> class.
    /// </summary>
    private ServerlessQueryRegistry() : base(att => att.EntityKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the <see cref="ServerlessQueryRegistry"/> class.
    /// </summary>
    public static ServerlessQueryRegistry Instance => ServerlessQueryRegistry.instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding IServerlessQueryRequest implementation for the given entity Kind.
    /// </summary>
    public IServerlessQueryRequest<BaseRecord, BaseEntity> CreateQueryFor(Type entityType,
        IServerlessQueryRequest<BaseRecord, BaseEntity> copyOf = null) => this.Resolve(entityType, copyOf);
}

