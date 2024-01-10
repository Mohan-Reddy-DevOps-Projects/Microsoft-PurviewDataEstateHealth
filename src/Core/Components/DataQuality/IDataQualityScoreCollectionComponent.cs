// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing data quality score collections.
/// </summary>
public interface IDataQualityScoreCollectionComponent :
    IComponent<IDataQualityListContext>
{
    /// <summary>
    /// Get all domain scores.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="skipToken"></param>
    /// <returns></returns>
    public Task<IBatchResults<DataQualityScoreModel>> GetDomainScores(
        CancellationToken cancellationToken,
        string skipToken = null);

    /// <summary>
    /// Get domain score by id.
    /// </summary>
    /// <param name="domainId"></param>
    /// <returns></returns>
    public IDataQualityScoreComponent GetDomainScoreById(
        Guid domainId);

    /// <summary>
    /// Get all data product scores from a specific domain.
    /// </summary>
    /// <param name="domainId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="skipToken"></param>
    /// <returns></returns>
    public Task<IBatchResults<DataQualityScoreModel>> GetDataProductScores(
        Guid domainId,
        CancellationToken cancellationToken,
        string skipToken = null);

    /// <summary>
    /// Get data product score by id.
    /// </summary>
    /// <param name="dataProductId"></param>
    /// <returns></returns>
    public IDataQualityScoreComponent GetDataProductScoreById(
        Guid dataProductId);

    /// <summary>
    /// Get all data asset scores from a specific data product.
    /// </summary>
    /// <param name="domainId"></param>
    /// <param name="dataProductId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="skipToken"></param>
    /// <returns></returns>
    public Task<IBatchResults<DataQualityScoreModel>> GetDataAssetScores(
        Guid domainId,
        Guid dataProductId,
        CancellationToken cancellationToken,
        string skipToken = null);

    /// <summary>
    /// Get data asset score by id.
    /// </summary>
    /// <param name="dataAssetId"></param>
    /// <returns></returns>
    public IDataQualityScoreComponent GetDataAssetScoreById(
        Guid dataAssetId);
}
