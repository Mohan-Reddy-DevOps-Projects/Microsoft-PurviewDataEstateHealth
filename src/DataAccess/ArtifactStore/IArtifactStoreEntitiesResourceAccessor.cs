// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Purview.ArtifactStoreClient;
using Microsoft.Purview.ArtifactStoreClient.Models;

/// <summary>
/// Interface for classes that interact with resources in the artifact store.
/// </summary>
internal interface IArtifactStoreEntitiesResourceAccessor
{
    Task<ArtifactStoreEntityDocument<TEntity>> CreateOrUpdateResourceAsync<TEntity>(
        string accountId,
        string entityId,
        string entityType,
        string correlationId,
        TEntity resourceEntity,
        string ifMatch = null,
        object indexedProperties = null,
        CancellationToken cancellationToken = new CancellationToken());

    Task<ArtifactStoreEntityQueryResult<TEntity>> QueryResourcesByCategoryAsync<TEntity>(
        string accountId,
        string entityType,
        List<string> filterText,
        Dictionary<string, string> parameters,
        OrderByClause orderByClause,
        string correlationId,
        int? maxPageSize = null,
        int offset = 0,
        int? limit = null,
        string groupByCondition = null,
        string projectionCondition = null,
        string continuationToken = null,
        CancellationToken cancellationToken = new CancellationToken(),
        bool byPassObligations = false);

    Task<ArtifactStoreEntityDocument<TEntity>> GetResourceAsync<TEntity>(
        string accountId,
        string entityType,
        string entityId,
        string correlationId,
        string ifNoneMatch = null,
        string projectionFilters = null,
        CancellationToken cancellationToken = new CancellationToken());

    Task<long> GetResourceCountByCategoryAsync(
        string accountId,
        string entityType,
        List<string> filterText,
        Dictionary<string, string> parameters,
        string correlationId,
        CancellationToken cancellationToken = new CancellationToken());

    Task DeleteResourceAsync(
        string accountId,
        string entityType,
        string entityId,
        string correlationId,
        string ifMatch = null,
        CancellationToken cancellationToken = new CancellationToken());
}
