// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.ArtifactStoreClient;
using Microsoft.Purview.ArtifactStoreClient.Models;
using AdfModels = Microsoft.Purview.ArtifactStoreClient.Models;

/// <summary>
/// A Shim over ArtifactStoreResourceAccessor implementing IArtifactStoreEntitiesResourceAccessor contract
/// to reduce burden on callers/maintain consistency for common params values e.g. correlationId.
/// This also facilitate writing better mocked tests for cross region artifact store operations.
/// </summary>
internal class ArtifactStoreEntitiesResourceAccessor : IArtifactStoreEntitiesResourceAccessor
{
    private readonly IDataEstateHealthRequestLogger genevaLogger;

    private readonly ArtifactStoreResourceAccessor artifactStoreResourceAccessor;

    private readonly string targetArtifactStore;

    private const string UserAgent = "DataEstateHealth";

    private static readonly TimeSpan DefaultConnectTimeout = TimeSpan.FromSeconds(5);

    private const string OperationStart = "ArtifactStoreServiceOperationStart";

    private const string OperationSuccess = "ArtifactStoreServiceOperationSuccess";

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactStoreEntitiesResourceAccessor" /> class.
    /// </summary>
    /// <param name="genevaLogger"></param>
    /// <param name="httpContextAccessor"></param>
    /// <param name="artifactStoreConfiguration"></param>
    public ArtifactStoreEntitiesResourceAccessor(
        IDataEstateHealthRequestLogger genevaLogger,
        IHttpContextAccessor httpContextAccessor,
        IArtifactStoreConfiguration artifactStoreConfiguration)
    {
        this.genevaLogger = genevaLogger;
        this.artifactStoreResourceAccessor = new ArtifactStoreResourceAccessor(
               artifactStoreConfiguration,
               socketsHttpHandler: new SocketsHttpHandler()
               {
                   ConnectTimeout = DefaultConnectTimeout,
               },
               disposeHandler: true,
               userAgent: UserAgent,
               contextAccessor: httpContextAccessor);
        this.targetArtifactStore = $"{artifactStoreConfiguration.BaseManagementUri}  ArtifactStore";
    }

    /// <inheritdoc />
    public async Task<ArtifactStoreEntityDocument<TEntity>> CreateOrUpdateResourceAsync<TEntity>(
        string accountId,
        string entityId,
        string entityType,
        string correlationId,
        TEntity resourceEntity,
        string ifMatch = null,
        object indexedProperties = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        this.LogArtifactStoreServiceEvent(
            OperationStart,
            this.targetArtifactStore,
            accountId,
            entityType,
            "CREATE",
            -1);

        ArtifactStoreEntityDocument<TEntity> createResourceResponse =
            await this.artifactStoreResourceAccessor.CreateOrUpdateResourceAsync<TEntity>(
                accountId: accountId,
                entityId: entityId,
                entityType: entityType,
                correlationId: correlationId,
                resourceEntity: resourceEntity,
                ifMatch: ifMatch,
                indexedProperties: indexedProperties,
                cancellationToken: cancellationToken);

        this.LogArtifactStoreServiceEvent(
            OperationSuccess,
            this.targetArtifactStore,
            accountId,
            entityType,
            "CREATE",
            createResourceResponse.Properties != null ? 1 : 0);

        return createResourceResponse;
    }

    /// <inheritdoc/>
    public async Task<ArtifactStoreEntityQueryResult<TEntity>> QueryResourcesByCategoryAsync<TEntity>(
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
        bool byPassObligations = false)
    {
        this.LogArtifactStoreServiceEvent(
            OperationStart,
            this.targetArtifactStore,
            accountId,
            entityType,
            "LIST",
            -1);

        AdfModels.OrderByClause orderBy = null;
        if (orderByClause != null)
        {
            orderBy = new AdfModels.OrderByClause(
                orderByClause.OrderByText,
                orderByClause.OrderByDirection == OrderByDirection.DESC
                    ? AdfModels.OrderByDirection.DESC
                    : AdfModels.OrderByDirection.ASC);
        }

        ArtifactStoreEntityQueryResult<TEntity> listResourcesResponse =
            await this.artifactStoreResourceAccessor.QueryResourcesByCategoryAsync<TEntity>(
                accountId: accountId,
                entityType: entityType,
                filterText: filterText,
                parameters: parameters,
                orderByClause: orderBy,
                correlationId: correlationId,
                maxPageSize: maxPageSize,
                offset: offset,
                limit: limit,
                groupByCondition: groupByCondition,
                projectionCondition: projectionCondition,
                continuationToken: continuationToken,
                cancellationToken: cancellationToken,
                bypassObligations: byPassObligations);

        this.LogArtifactStoreServiceEvent(
            OperationSuccess,
            this.targetArtifactStore,
            accountId,
            entityType,
            "LIST",
            listResourcesResponse.Items.Count());

        return listResourcesResponse;
    }

    /// <inheritdoc/>
    public async Task<ArtifactStoreEntityDocument<TEntity>> GetResourceAsync<TEntity>(
        string accountId,
        string entityType,
        string entityId,
        string correlationId,
        string ifNoneMatch = null,
        string projectionFilters = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        this.LogArtifactStoreServiceEvent(
            OperationStart,
            this.targetArtifactStore,
            accountId,
            entityType,
            "GET",
            -1);

        ArtifactStoreEntityDocument<TEntity> getResourceResponse =
            await this.artifactStoreResourceAccessor.GetResourceAsync<TEntity>(
                accountId: accountId,
                entityType: entityType,
                entityId: entityId,
                correlationId: correlationId,
                ifNoneMatch: ifNoneMatch,
                projectionFilters: projectionFilters,
                cancellationToken: cancellationToken);

        this.LogArtifactStoreServiceEvent(
            OperationSuccess,
            this.targetArtifactStore,
            accountId,
            entityType,
            "GET",
            getResourceResponse.Properties != null ? 1 : 0);

        return getResourceResponse;
    }

    /// <inheritdoc/>
    public async Task<long> GetResourceCountByCategoryAsync(
        string accountId,
        string entityType,
        List<string> filterText,
        Dictionary<string, string> parameters,
        string correlationId,
        CancellationToken cancellationToken = new CancellationToken())
    {
        this.LogArtifactStoreServiceEvent(
            OperationStart,
            this.targetArtifactStore,
            accountId,
            entityType,
            "COUNT",
            -1);

        long count = await this.artifactStoreResourceAccessor.GetResourceCountByCategoryAsync(
            accountId: accountId,
            entityType: entityType,
            correlationId: correlationId,
            filterText: filterText,
            parameters: parameters,
            cancellationToken: cancellationToken);

        this.LogArtifactStoreServiceEvent(
            OperationSuccess,
            this.targetArtifactStore,
            accountId,
            entityType,
            "COUNT",
            count);

        return count;
    }

    /// <inheritdoc/>
    public async Task DeleteResourceAsync(
        string accountId,
        string entityType,
        string entityId,
        string correlationId,
        string ifMatch = null,
        CancellationToken cancellationToken = new CancellationToken())
    {
        this.LogArtifactStoreServiceEvent(
            OperationStart,
            this.targetArtifactStore,
            accountId,
            entityType,
            "DELETE",
            -1);

        await this.artifactStoreResourceAccessor.DeleteResourceAsync(
            accountId: accountId,
            entityType: entityType,
            entityId: entityId,
            correlationId: correlationId,
            ifMatch: ifMatch,
            cancellationToken: cancellationToken);

        this.LogArtifactStoreServiceEvent(
            OperationSuccess,
            this.targetArtifactStore,
            accountId,
            entityType,
            "DELETE",
            -1);
    }

    private void LogArtifactStoreServiceEvent(string operation, string store, string partition, string resourceType,
        string storeOperation, long? count)
    {
        string information = $"Operation = {operation ?? string.Empty} \n" +
            $"Store = {store ?? string.Empty} \n" +
            $"Partition = {partition ?? string.Empty} \n" +
            $"ResourceType = {resourceType ?? string.Empty} \n" +
            $"StoreOperation = {storeOperation ?? string.Empty} \n" +
            $"Count = {count}";

        this.genevaLogger.LogInformation(information);
    }
}
