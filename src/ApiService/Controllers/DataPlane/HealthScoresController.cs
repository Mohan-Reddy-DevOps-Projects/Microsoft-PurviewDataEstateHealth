// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using global::Microsoft.AspNetCore.Mvc;
using global::Microsoft.Azure.Purview.DataEstateHealth.Core;
using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;
using global::Microsoft.DGP.ServiceBasics.BaseModels;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Health scores controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/scores/businessDomains/")]
public class HealthScoresController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Health scores controller constructor
    /// </summary>
    public HealthScoresController(
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
    /// Get health score for a given business domain.
    /// </summary>
    /// <param name="businessDomainId">domain id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{businessDomainId}")]
    [ProducesResponseType(typeof(HealthScore), 200)]
    public async Task<IActionResult> GetHealthScoreByIDomainIdAsync(
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
    /// Get health scores for all business domains.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthScoreList), 200)]
    public async Task<IActionResult> ListHealthScoresAsync(
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

