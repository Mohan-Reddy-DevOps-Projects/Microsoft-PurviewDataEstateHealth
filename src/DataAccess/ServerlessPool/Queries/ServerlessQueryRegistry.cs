// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Registry for queries
/// </summary>
public class ServerlessQueryRegistry<TRecord, TEntity> : Registry<ServerlessQueryAttribute, Type, IServerlessQueryRequest<TRecord, TEntity>>
{
    private static readonly Lazy<ServerlessQueryRegistry<TRecord, TEntity>> instance =
            new Lazy<ServerlessQueryRegistry<TRecord, TEntity>>(() => new ServerlessQueryRegistry<TRecord, TEntity>());

    /// <summary>
    /// Prevents creating instances of the class.
    /// </summary>
    private ServerlessQueryRegistry() : base(att => att.EntityKind)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the class.
    /// </summary>
    public static ServerlessQueryRegistry<TRecord, TEntity> Instance => instance.Value;

    /// <summary>
    /// Returns an instance of the corresponding IServerlessQueryRequest implementation for the given entity Kind.
    /// </summary>
    public IServerlessQueryRequest<TRecord, TEntity> CreateQueryFor(Type entityType,
        IServerlessQueryRequest<TRecord, TEntity> copyOf = null) => this.Resolve(entityType, copyOf);
}

