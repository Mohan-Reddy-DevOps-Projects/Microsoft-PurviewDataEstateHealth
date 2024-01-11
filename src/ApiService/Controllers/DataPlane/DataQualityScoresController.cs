// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Data quality scores controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/scores/dataQuality/")]
public class DataQualityScoresController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Data Quality scores controller constructor
    /// </summary>
    public DataQualityScoresController(
        ICoreLayerFactory coreLayerFactory,
        ModelAdapterRegistry adapterRegistry,
        IRequestHeaderContext requestHeaderContext,
        ControllerContext controllerContext = null)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.adapterRegistry = adapterRegistry;
        this.requestHeaderContext = requestHeaderContext;

        if (controllerContext != null)
        {
            this.ControllerContext = controllerContext;
        }
    }

    /// <summary>
    /// Get data quality score for a given business domain.
    /// </summary>
    /// <param name="businessDomainId">Business domain id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("businessDomains/{businessDomainId}")]
    [ProducesResponseType(typeof(BusinessDomainDataQualityScore), 200)]
    public async Task<IActionResult> GetDataQualityScoreByDomainIdAsync(
        [FromRoute] Guid businessDomainId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        DataQualityScoresModel dataQualityScoresModel
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateDataQualityScoresCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .GetDomainScoreById(businessDomainId)
                .Get(cancellationToken);

        return this.Ok(
                   this.adapterRegistry.AdapterFor<IDataQualityScoresModel, BusinessDomainDataQualityScore>()
                   .FromModel(dataQualityScoresModel));
    }

    /// <summary>
    /// Get data quality scores for all business domains.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("businessDomains")]
    [ProducesResponseType(typeof(BusinessDomainDataQualityScoreList), 200)]
    public async Task<IActionResult> ListDataQualityDomainScoresAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IDataQualityScoresModel> results = await this.coreLayerFactory
            .Of(ServiceVersion.From(apiVersion))
            .CreateDataQualityScoresCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .GetDomainScores(cancellationToken, skipToken);

        var businessDomainQualityScores = new BusinessDomainDataQualityScoreList
        {
            Value = results.Results.Select(
            businessDomainDataQualityScore => this.adapterRegistry.AdapterFor<IDataQualityScoresModel, BusinessDomainDataQualityScore>()
            .FromModel(businessDomainDataQualityScore))
            .ToList()
        };

        return this.Ok(businessDomainQualityScores);
    }

    /// <summary>
    /// Get data quality score for a given data product.
    /// </summary>
    /// <param name="dataProductId">Data product id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("dataProducts/{dataProductId}")]
    [ProducesResponseType(typeof(ProductDataQualityScore), 200)]
    public async Task<IActionResult> GetDataQualityScoresByDataProductIdAsync(
        [FromRoute] Guid dataProductId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        DataQualityScoresModel dataQualityScoresModel = await this.coreLayerFactory
            .Of(ServiceVersion.From(apiVersion))
            .CreateDataQualityScoresCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .GetDataProductScoreById(
                dataProductId)
            .Get(cancellationToken);

        return this.Ok(
                  this.adapterRegistry.AdapterFor<IDataQualityScoresModel, ProductDataQualityScore>()
                  .FromModel(dataQualityScoresModel));
    }

    /// <summary>
    /// Get data quality scores for all data products in a given domain.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("dataProducts")]
    [ProducesResponseType(typeof(ProductDataQualityScoreList), 200)]
    public async Task<IActionResult> ListDataQualityDataProductScoresAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IDataQualityScoresModel> results = await this.coreLayerFactory
            .Of(ServiceVersion.From(apiVersion))
            .CreateDataQualityScoresCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .GetDataProductScores(
                cancellationToken,
                skipToken);

        var dataProductQualityScores = new ProductDataQualityScoreList
        {
            Value = results.Results.Select(
           dataProductQualityScore => this.adapterRegistry.AdapterFor<IDataQualityScoresModel, ProductDataQualityScore>()
           .FromModel(dataProductQualityScore))
           .ToList()
        };

        return this.Ok(dataProductQualityScores);
    }

    /// <summary>
    /// Get data quality scores for a given asset.
    /// </summary>
    /// <param name="dataAssetId">Data asset id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("dataAssets/{dataAssetId}")]
    [ProducesResponseType(typeof(AssetDataQualityScore), 200)]
    public async Task<IActionResult> GetDataQualityScoresByDataAssetIdAsync(
        [FromRoute] Guid dataAssetId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        DataQualityScoresModel dataQualityScoresModel = await this.coreLayerFactory
            .Of(ServiceVersion.From(apiVersion))
            .CreateDataQualityScoresCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .GetDataAssetScoreById(dataAssetId)
            .Get(cancellationToken);

        return this.Ok(
                  this.adapterRegistry.AdapterFor<IDataQualityScoresModel, AssetDataQualityScore>()
                  .FromModel(dataQualityScoresModel));
    }

    /// <summary>
    /// Get data quality scores for all assets in a given data product.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("dataAssets")]
    [ProducesResponseType(typeof(AssetDataQualityScoreList), 200)]
    public async Task<IActionResult> ListDataQualityDataAssetScoresAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IDataQualityScoresModel> results = await this.coreLayerFactory
            .Of(ServiceVersion.From(apiVersion))
            .CreateDataQualityScoresCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .GetDataAssetScores(
                cancellationToken,
                skipToken);

        var assetQualityScores = new AssetDataQualityScoreList
        {
            Value = results.Results.Select(
          assetQualityScore => this.adapterRegistry.AdapterFor<IDataQualityScoresModel, AssetDataQualityScore>()
          .FromModel(assetQualityScore))
          .ToList()
        };

        return this.Ok(assetQualityScores);
    }
}
