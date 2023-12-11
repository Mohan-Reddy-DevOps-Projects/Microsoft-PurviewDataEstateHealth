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
/// Data Quality scores controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/scores/dataQuality/businessDomains/")]
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
    [Route("{businessDomainId}")]
    [ProducesResponseType(typeof(DataQualityScore), 200)]
    public async Task<IActionResult> GetDataQualityScoreByIDomainIdAsync(
        [FromRoute] Guid businessDomainId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> healthScoreModelResults
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateHealthScoreCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .ById(businessDomainId)
                .Get(cancellationToken);

        var healthScores = new HealthScoreList
        {
            Value = healthScoreModelResults.Results.Select(
            healthScore => this.adapterRegistry.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>()
            .FromModel(healthScore))
            .ToList()
        };

        return this.Ok(healthScores);
    }

    /// <summary>
    /// Get data quality scores for all business domains.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthScoreList), 200)]
    public async Task<IActionResult> ListDataQualityDomainScoresAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> results = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthScoreCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(cancellationToken, skipToken);

        var healthScores = new HealthScoreList
        {
            Value = results.Results.Select(
            healthScore => this.adapterRegistry.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>()
            .FromModel(healthScore))
            .ToList()
        };

        return this.Ok(healthScores);
    }

    /// <summary>
    /// Get data quality score for a given data product.
    /// </summary>
    /// <param name="businessDomainId">Business domain id.</param>
    /// <param name="dataProductId">Data product id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{businessDomainId}/dataProducts/{dataProductId}")]
    [ProducesResponseType(typeof(DataQualityScore), 200)]
    public async Task<IActionResult> GetDataQualityScoresByDataProductIdAsync(
        [FromRoute] Guid businessDomainId,
        [FromRoute] Guid dataProductId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> healthScoreModelResults
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateHealthScoreCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .ById(businessDomainId)
                .Get(cancellationToken);

        var healthScores = new HealthScoreList
        {
            Value = healthScoreModelResults.Results.Select(
            healthScore => this.adapterRegistry.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>()
            .FromModel(healthScore))
            .ToList()
        };

        return this.Ok(healthScores);
    }

    /// <summary>
    /// Get data quality scores for all data products in a given domain.
    /// </summary>
    /// <param name="businessDomainId">Business Domain Id</param>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{businessDomainId}/dataProducts")]
    [ProducesResponseType(typeof(HealthScoreList), 200)]
    public async Task<IActionResult> ListDataQualityDataProductScoresAsync(
        [FromRoute] string businessDomainId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> results = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthScoreCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(cancellationToken, skipToken);

        var healthScores = new HealthScoreList
        {
            Value = results.Results.Select(
            healthScore => this.adapterRegistry.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>()
            .FromModel(healthScore))
            .ToList()
        };

        return this.Ok(healthScores);
    }

    /// <summary>
    /// Get data quality scores for a given asset.
    /// </summary>
    /// <param name="businessDomainId">Business Domain id.</param>
    /// <param name="dataProductId">Data product id.</param>
    /// <param name="dataAssetId">Data asset id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{businessDomainId}/dataProducts/{dataProductId}/dataAssets/{dataAssetId}")]
    [ProducesResponseType(typeof(DataQualityScore), 200)]
    public async Task<IActionResult> GetHealthScoreByIDomainIdAsync(
        [FromRoute] Guid businessDomainId,
        [FromRoute] Guid dataProductId,
        [FromRoute] Guid dataAssetId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> healthScoreModelResults
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateHealthScoreCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .ById(businessDomainId)
                .Get(cancellationToken);

        var healthScores = new HealthScoreList
        {
            Value = healthScoreModelResults.Results.Select(
            healthScore => this.adapterRegistry.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>()
            .FromModel(healthScore))
            .ToList()
        };

        return this.Ok(healthScores);
    }

    /// <summary>
    /// Get data quality scores for all assets in a given data product.
    /// </summary>
    /// <param name="businessDomainId">Business domain id.</param>
    /// <param name="dataProductId">Data product id.</param>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{businessDomainId}/dataProducts/{dataProductId}/dataAssets")]
    [ProducesResponseType(typeof(HealthScoreList), 200)]
    public async Task<IActionResult> ListHealthScoresAsync(
        [FromRoute] Guid businessDomainId,
        [FromRoute] Guid dataProductId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IHealthScoreModel<HealthScoreProperties>> results = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthScoreCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(cancellationToken, skipToken);

        var healthScores = new HealthScoreList
        {
            Value = results.Results.Select(
            healthScore => this.adapterRegistry.AdapterFor<IHealthScoreModel<HealthScoreProperties>, HealthScore>()
            .FromModel(healthScore))
            .ToList()
        };

        return this.Ok(healthScores);
    }
}
