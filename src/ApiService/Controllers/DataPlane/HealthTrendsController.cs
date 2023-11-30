// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Asp.Versioning;
using TrendKind = DataTransferObjects.TrendKind;

/// <summary>
/// Health trends controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/trends/{trendKind}/businessDomains/")]
public class HealthTrendsController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Health trends controller constructor
    /// </summary>
    public HealthTrendsController(
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
    /// Get health trends for all business domains based on the trend kind.
    /// </summary>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="trendKind">Trend Kind. </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthTrend), 200)]
    public async Task<IActionResult> GetHealthTrendsAsync(
        [FromRoute] TrendKind trendKind,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IHealthTrendModel healthTrendModel = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthTrendComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(trendKind.ToModel(), cancellationToken);

        return this.Ok(
                   this.adapterRegistry.AdapterFor<IHealthTrendModel, HealthTrend>()
                   .FromModel(healthTrendModel));
    }

    /// <summary>
    /// Get health trends for a specific business domain based on the trend kind.
    /// </summary>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="trendKind">Trend Kind. </param>
    /// <param name="businessDomainId">Business domain id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthTrend), 200)]
    [Route("{businessDomainId}")]
    public async Task<IActionResult> GetHealthTrendsByDomainIdAsync(
        [FromRoute] TrendKind trendKind,
        [FromRoute] Guid businessDomainId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IHealthTrendModel healthTrendModel = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthTrendComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .ById(businessDomainId)
            .Get(trendKind.ToModel(), cancellationToken);

        return this.Ok(
                this.adapterRegistry.AdapterFor<IHealthTrendModel, HealthTrend>()
                .FromModel(healthTrendModel));
    }
}

