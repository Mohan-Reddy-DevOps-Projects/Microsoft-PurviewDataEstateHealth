// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Interface for Synapse Serverless Pool executor.
/// </summary>
public interface IServerlessQueryExecutor
{
    /// <summary>
    /// Execute query request method.
    /// </summary>
    Task<IList<TEntity>> ExecuteAsync<TIntermediate, TEntity>(IServerlessQueryRequest<TIntermediate, TEntity> request, CancellationToken cancellationToken)
        where TEntity : BaseEntity where TIntermediate : BaseRecord;
}
