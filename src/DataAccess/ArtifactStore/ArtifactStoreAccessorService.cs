// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Shared;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Purview.ArtifactStoreClient;
using Microsoft.Purview.ArtifactStoreClient.Models;
using Microsoft.Rest.TransientFaultHandling;
using Polly;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

internal class ArtifactStoreAccessorService : IArtifactStoreAccessorService
{
    private readonly IArtifactStoreEntitiesResourceAccessor artifactStoreEntitiesResourceAccessor;

    private readonly IDataEstateHealthRequestLogger genevaLogger;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly RetryPolicy<TransientErrorIgnoreStrategy> noRetryPolicy = new(0);

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtifactStoreAccessorService" /> class.
    /// </summary>
    /// <param name="artifactStoreResourceAccessor"></param>
    /// <param name="genevaLogger"></param>
    /// <param name="requestHeaderContext"></param>
    public ArtifactStoreAccessorService(
        IArtifactStoreEntitiesResourceAccessor artifactStoreResourceAccessor,
        IDataEstateHealthRequestLogger genevaLogger,
        IRequestHeaderContext requestHeaderContext)
    {
        this.artifactStoreEntitiesResourceAccessor = artifactStoreResourceAccessor;
        this.genevaLogger = genevaLogger;
        this.requestHeaderContext = requestHeaderContext;
    }

    /// <inheritdoc/>
    public async Task<ArtifactStoreEntityDocument<TEntity>> GetResourceAsync<TEntity>(
        Guid accountId,
        string entityId,
        string entityType,
        string ifNoneMatch = null,
        string projectionFilters = null,
        CancellationToken cancellationToken = default)
    {
        return await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.genevaLogger, nameof(ArtifactStoreAccessorService)))
            .AsAsyncPolicy<ArtifactStoreEntityDocument<TEntity>>()
            .ExecuteAsync(() =>
                this.RunArtifactOperationAsync(() =>
                    this.artifactStoreEntitiesResourceAccessor.GetResourceAsync<TEntity>(
                        accountId.ToString(),
                        entityType,
                        entityId.ToString(),
                        this.requestHeaderContext.CorrelationId,
                        ifNoneMatch,
                        projectionFilters,
                        cancellationToken),
                    entityType,
                    "GetResourceAsync"));
    }

    /// <inheritdoc/>
    public async Task<long> GetResourceCountByCategoryAsync(
        Guid accountId,
        string entityType,
        List<string> filterText = null,
        Dictionary<string, string> parameters = null,
        CancellationToken cancellationToken = default)
    {
        return await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.genevaLogger, nameof(ArtifactStoreAccessorService)))
            .AsAsyncPolicy<long>()
            .ExecuteAsync(() =>
                this.RunArtifactOperationAsync(() =>
                    this.artifactStoreEntitiesResourceAccessor.GetResourceCountByCategoryAsync(
                        accountId.ToString(),
                        entityType,
                        filterText,
                        parameters,
                        this.requestHeaderContext.CorrelationId,
                        cancellationToken),
                    entityType,
                    "GetResourceCountByCategoryAsync"));
    }

    /// <inheritdoc/>
    public async Task<ArtifactStoreEntityQueryResult<TEntity>> ListResourcesByCategoryAsync<TEntity>(
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
        bool byPassObligations = false)
    {
        OrderByClause orderByClause = null;
        if (!string.IsNullOrWhiteSpace(orderByText))
        {
            orderByClause = new OrderByClause(
                orderByText,
                orderAscending ? OrderByDirection.ASC : OrderByDirection.DESC);
        }

        return await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.genevaLogger, nameof(ArtifactStoreAccessorService)))
            .AsAsyncPolicy<ArtifactStoreEntityQueryResult<TEntity>>()
            .ExecuteAsync(() =>
                this.RunArtifactOperationAsync(() =>
                    this.artifactStoreEntitiesResourceAccessor.QueryResourcesByCategoryAsync<TEntity>(
                        accountId.ToString(),
                        entityType,
                        filterText,
                        parameters,
                        orderByClause,
                        this.requestHeaderContext.CorrelationId,
                        maxPageSize,
                        offset,
                        limit,
                        groupByCondition,
                        projectionCondition,
                        continuationToken,
                        cancellationToken,
                        byPassObligations),
                    entityType,
                    "QueryResourcesByCategoryAsync",
                    new ArtifactStoreEntityQueryResult<TEntity>() { Items = new List<ArtifactStoreEntityDocument<TEntity>>() }));
    }

    /// <inheritdoc/>
    public async Task<ArtifactStoreEntityDocument<TEntity>> CreateOrUpdateResourceAsync<TEntity>(
        Guid accountId,
        string entityId,
        string entityType,
        TEntity entity,
        string ifMatch = null,
        Dictionary<string, string> indexedProperties = null,
        CancellationToken cancellationToken = default)
    {
        return await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.genevaLogger, nameof(ArtifactStoreAccessorService)))
            .AsAsyncPolicy<ArtifactStoreEntityDocument<TEntity>>()
            .ExecuteAsync(() =>
                this.RunArtifactOperationAsync(() =>
                    this.artifactStoreEntitiesResourceAccessor.CreateOrUpdateResourceAsync(
                        accountId.ToString(),
                        entityId.ToString(),
                        entityType,
                        this.requestHeaderContext.CorrelationId,
                        entity,
                        ifMatch,
                        indexedProperties,
                        cancellationToken),
                    entityType,
                    "CreateOrUpdateResourceAsync"));
    }

    /// <inheritdoc/>
    public async Task DeleteResourceAsync(
        Guid accountId,
        string entityId,
        string entityType,
        string ifMatch = null,
        CancellationToken cancellationToken = default)
    {
        await PollyRetryPolicies
            .GetNonHttpClientTransientRetryPolicy(
                LoggerRetryActionFactory.CreateWorkerRetryAction(this.genevaLogger, nameof(ArtifactStoreAccessorService)))
            .ExecuteAsync(() =>
                this.RunArtifactOperationAsync(() => Task.FromResult(
                    this.artifactStoreEntitiesResourceAccessor.DeleteResourceAsync(
                        accountId.ToString(),
                        entityType,
                        entityId.ToString(),
                        this.requestHeaderContext.CorrelationId,
                        ifMatch,
                        cancellationToken)
                    .Wait(Timeout.Infinite)),
                entityType,
                "DeleteResourceAsync"));
    }

    /// <summary>
    /// Handles exception thrown while accessing the artifact store.
    /// </summary>
    /// <param name="exception">The exception</param>
    /// <param name="entityType">The type of entity involved in the artifact store operation</param>
    /// <returns>An exception</returns>
    private ServiceException HandleException(Exception exception, string entityType)
    {
        // Logging as error to avoid multiple criticals for each try. Higher level caller will log critical on failure.
        this.genevaLogger.LogError(
            $"ArtifactStoreAccessorService: failed to perform operation on {entityType}",
            exception);

        return new ServiceException(
            new ServiceError(
                ErrorCategory.ServiceError,
                ErrorCode.ArtifactStoreServiceException.Code,
                ErrorMessage.DownstreamDependency));
    }

    /// <summary>
    /// Run an artifact store CRUD operation.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="operation">Operation to invoke.</param>
    /// <param name="entityType">Entity type being operated on.</param>
    /// <param name="operationName">Name of operation being performed.</param>
    /// <param name="defaultResult">Default result to return in case of handled error.</param>
    /// <returns></returns>
    private async Task<TResult> RunArtifactOperationAsync<TResult>(
        Func<Task<TResult>> operation,
        string entityType,
        string operationName,
        TResult defaultResult = default)
    {
        Exception traceException = null;
        var stopWatch = Stopwatch.StartNew();

        try
        {
            return await operation();
        }
        catch (ArtifactStoreException exception) when (exception.ArtifactStoreErrorCode ==
                                                        ArtifactStoreErrorCode.DataFactoryDoesNotExist)
        {
            traceException = exception;
            this.genevaLogger.LogWarning("The data factory associated with Purview account does not exist. Possibly Purview account provisioning failed.");
            return defaultResult;
        }
        catch (ArtifactStoreException exception) when (exception.ArtifactStoreErrorCode ==
                                                       ArtifactStoreErrorCode.EntityNotFound)
        {
            traceException = exception;
            return defaultResult;
        }
        catch (ErrorResponseException exception) when
            (exception.Response.StatusCode is HttpStatusCode.InternalServerError &&
             exception.Response.Content.Contains("'databaseId' may not be null"))
        {
            traceException = exception;
            this.genevaLogger.LogWarning("The data factory associated with Purview account does not exist. Possibly Purview account provisioning failed, or you are trying to access an account using the incorrect artifact store location endpoint.");
            return defaultResult;
        }
        catch (ErrorResponseException exception) when
            (exception.Response.StatusCode is HttpStatusCode.InternalServerError)
        {
            throw traceException = new ServiceError(
                    ErrorCategory.ServiceError,
                    ErrorCode.ArtifactStoreServiceException.Code,
                    ErrorMessage.DownstreamDependency)
                .ToException();
        }
        catch (Exception exception)
        {
            throw traceException = this.HandleException(exception, entityType);
        }
    }
}
