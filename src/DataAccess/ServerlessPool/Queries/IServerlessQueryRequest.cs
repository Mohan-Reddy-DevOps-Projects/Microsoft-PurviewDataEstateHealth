// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Data;

/// <summary>
/// Interface for ServerlessQueryRequest
/// </summary>
public interface IServerlessQueryRequest<out TIntermediate, out TEntity>
{
    /// <summary>
    /// Database
    /// </summary>
    string Database { get; set; }

    /// <summary>
    /// Container path
    /// </summary>
    string ContainerPath { get; set; }

    /// <summary>
    /// Query
    /// </summary>
    string Query { get; }

    /// <summary>
    /// Filter clause.
    /// </summary>
    string FilterClause { get; set; }

    /// <summary>
    /// Query path
    /// </summary>
    string QueryPath { get; }

    /// <summary>
    /// Parse row.
    /// </summary>
    TIntermediate ParseRow(IDataRecord row);

    /// <summary>
    /// Finalize.
    /// </summary>
    public IEnumerable<TEntity> Finalize(dynamic items);
}
