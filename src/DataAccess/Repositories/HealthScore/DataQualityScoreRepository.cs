// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;

/// <inheritdoc/>
internal class DataQualityScoreRepository : IDataQualityScoreRepository
{
    private readonly ModelAdapterRegistry modelAdapterRegistry;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly string location;

    private readonly IServerlessQueryExecutor queryExecutor;

    private readonly IServerlessQueryRequestBuilder queryRequestBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataQualityScoreRepository" /> class.
    /// </summary>
    /// <param name="modelAdapterRegistry"></param>
    /// <param name="processingStorageManager"></param>
    /// <param name="queryExecutor"></param>
    /// <param name="queryRequestBuilder"></param>
    /// <param name="location"></param>
    public DataQualityScoreRepository(
         ModelAdapterRegistry modelAdapterRegistry,
         IProcessingStorageManager processingStorageManager,
         IServerlessQueryExecutor queryExecutor,
         IServerlessQueryRequestBuilder queryRequestBuilder,
         string location = null)
    {
        this.modelAdapterRegistry = modelAdapterRegistry;
        this.processingStorageManager = processingStorageManager;
        this.queryExecutor = queryExecutor;
        this.queryRequestBuilder = queryRequestBuilder;
        this.location = location;
    }

    /// <inheritdoc/>
    public async Task<DataQualityScoresModel> GetSingle(
        DomainDataQualityScoreKey key,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(new DataQualityScoresModel());
    }

    /// <inheritdoc/>
    public async Task<DataQualityScoresModel> GetSingle(
        DataProductDataQualityScoreKey key,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(new DataQualityScoresModel());
    }

    /// <inheritdoc/>
    public async Task<DataQualityScoresModel> GetSingle(
        DataAssetDataQualityScoreKey key,
        CancellationToken cancellationToken)
    {
        return await Task.FromResult(new DataQualityScoresModel());
    }

    public Task<IBatchResults<DataQualityScoreModel>> GetMultiple(
        DomainDataQualityScoreKey criteria,
        CancellationToken cancellationToken,
        string continuationToken = null) => throw new NotImplementedException();

    public Task<IBatchResults<DataQualityScoreModel>> GetMultiple(
        DataProductDataQualityScoreKey criteria,
        CancellationToken cancellationToken,
        string continuationToken = null) => throw new NotImplementedException();

    public Task<IBatchResults<DataQualityScoreModel>> GetMultiple(
        DataAssetDataQualityScoreKey criteria,
        CancellationToken cancellationToken,
        string continuationToken = null) => throw new NotImplementedException();

    /// <inheritdoc/>
    public IDataQualityScoreRepository ByLocation(string location)
    {
        return new DataQualityScoreRepository(
            this.modelAdapterRegistry,
            this.processingStorageManager,
            this.queryExecutor,
            this.queryRequestBuilder,
            location);
    }
}
