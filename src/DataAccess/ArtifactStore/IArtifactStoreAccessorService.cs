// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using Microsoft.Purview.ArtifactStoreClient;

/// <summary>
/// Artifact store service class.
/// </summary>
public interface IArtifactStoreAccessorService
{
    /// <summary>
    /// Gets a resource
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="accountId">The Purview account ID</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="entityType">The type of the entity</param>
    /// <param name="ifNoneMatch">The if-none-match value for etag concurrency control</param>
    /// <param name="projectionFilters">Comma-separated projection filters</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The resource</returns>
    Task<ArtifactStoreEntityDocument<TEntity>> GetResourceAsync<TEntity>(
        Guid accountId,
        string entityId,
        string entityType,
        string ifNoneMatch = null,
        string projectionFilters = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Count resources by category.
    /// </summary>
    /// <param name="accountId">The Purview account ID</param>
    /// <param name="entityType">The type of the entity</param>
    /// <param name="filterText">The filter text in the form "E.ResourceType = @resourceType"</param>
    /// <param name="parameters">The parameters in the form ("@resourceType", entityType)</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A count of the specified resources</returns>
    Task<long> GetResourceCountByCategoryAsync(
        Guid accountId,
        string entityType,
        List<string> filterText,
        Dictionary<string, string> parameters,
        CancellationToken cancellationToken = new CancellationToken());

    /// <summary>
    /// Lists resources by category (entity type)
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="accountId">The Purview account ID</param>
    /// <param name="entityType">The type of the entity</param>
    /// <param name="filterText">The filter text in the form "E.ResourceType = @resourceType"</param>
    /// <param name="parameters">The parameters in the form ("@resourceType", entityType)</param>
    /// <param name="orderByText">The order by text</param>
    /// <param name="orderAscending">The order of the order by clause. Defaults to true</param>
    /// <param name="maxPageSize">The maximum number of results in a page</param>
    /// <param name="offset">The offset to start at in the result set</param>
    /// <param name="limit">The maximum number of results to retrieve</param>
    /// <param name="groupByCondition">The group by condition</param>
    /// <param name="projectionCondition">The projection condition</param>
    /// <param name="continuationToken">The continuation token</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="byPassObligations">A bool indicating whether obligations should be bypassed.</param>
    /// <returns>A list of resources</returns>
    Task<ArtifactStoreEntityQueryResult<TEntity>> ListResourcesByCategoryAsync<TEntity>(
        Guid accountId,
        string entityType,
        List<string> filterText = null,
        Dictionary<string, string> parameters = null,
        string orderByText = null,
        bool orderAscending = true,
        int maxPageSize = 250,
        int offset = 0,
        int? limit = null,
        string groupByCondition = null,
        string projectionCondition = null,
        string continuationToken = null,
        CancellationToken cancellationToken = default,
        bool byPassObligations = false);

    /// <summary>
    /// Creates or updates a resource
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="accountId">The Purview account ID</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="entityType">The type of the entity</param>
    /// <param name="entity">The resource entity to persist</param>
    /// <param name="ifMatch">The if-match value for etag concurrency control</param>
    /// <param name="indexedProperties">A set of properties that is stored uncompressed</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The resource</returns>
    Task<ArtifactStoreEntityDocument<TEntity>> CreateOrUpdateResourceAsync<TEntity>(
        Guid accountId,
        string entityId,
        string entityType,
        TEntity entity,
        string ifMatch = null,
        Dictionary<string, string> indexedProperties = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a resource
    /// </summary>
    /// <param name="accountId">The Purview account ID</param>
    /// <param name="entityId">The entity ID</param>
    /// <param name="entityType">The type of the entity</param>
    /// <param name="ifMatch">The if-match value for etag concurrency control</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task DeleteResourceAsync(
        Guid accountId,
        string entityId,
        string entityType,
        string ifMatch = null,
        CancellationToken cancellationToken = default);
}
