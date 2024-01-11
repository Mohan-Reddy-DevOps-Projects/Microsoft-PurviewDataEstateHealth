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
    public Task<IBatchResults<IDataQualityScoresModel>> GetDomainScores(
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
    /// <param name="cancellationToken"></param>
    /// <param name="skipToken"></param>
    /// <returns></returns>
    public Task<IBatchResults<IDataQualityScoresModel>> GetDataProductScores(
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
    /// <param name="cancellationToken"></param>
    /// <param name="skipToken"></param>
    /// <returns></returns>
    public Task<IBatchResults<IDataQualityScoresModel>> GetDataAssetScores(
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
