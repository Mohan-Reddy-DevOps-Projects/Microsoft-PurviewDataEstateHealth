// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.DGP.ServiceBasics.BaseModels;
using Asp.Versioning;

/// <summary>
/// Health Actions controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/actions/businessDomains/")]
public class HealthActionsController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Health Actions controller constructor
    /// </summary>
    public HealthActionsController(
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
    /// Get health actions for all business domains.
    /// </summary>
    /// <param name="businessDomainId">Business domain id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{businessDomainId}")]
    [ProducesResponseType(typeof(HealthActionList), 200)]
    public async Task<IActionResult> GetHealthActionByIDomainIdAsync(
        [FromRoute] Guid businessDomainId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IBatchResults<IHealthActionModel> healthActionModelResults
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateHealthActionCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .ById(businessDomainId)
                .Get(cancellationToken);

        var healthActions = new HealthActionList
        {
            Value = healthActionModelResults.Results.Select(
            healthAction => this.adapterRegistry.AdapterFor<IHealthActionModel, HealthAction>()
            .FromModel(healthAction))
            .ToList()
        };

        return this.Ok(healthActions);
    }

    /// <summary>
    /// Get health action by business domain Id.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthActionList), 200)]
    public async Task<IActionResult> ListHealthActionsAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IHealthActionModel> results = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthActionCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(cancellationToken, skipToken);

        var healthActions = new HealthActionList
        {
            Value = results.Results.Select(
            healthAction => this.adapterRegistry.AdapterFor<IHealthActionModel, HealthAction>()
            .FromModel(healthAction))
            .ToList()
        };

        return this.Ok(healthActions);
    }
}

