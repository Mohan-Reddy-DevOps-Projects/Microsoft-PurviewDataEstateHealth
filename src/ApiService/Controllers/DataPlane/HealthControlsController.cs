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
/// Health controls controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/controls/")]
public class HealthControlsController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Health controls controller constructor
    /// </summary>
    public HealthControlsController(
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
    /// List all health controls.
    /// </summary>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthControlList), 200)]
    public async Task<IActionResult> GetHealthControlsAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IBatchResults<HealthControlModel> healthControlModelResults
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateHealthControlCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .Get(cancellationToken);

        var healthControls = new HealthControlList
        {
            Value = healthControlModelResults.Results.Select(
            healthControl => this.adapterRegistry.AdapterFor<HealthControlModel, HealthControl>()
            .FromModel(healthControl))
            .ToList()
        };

        return this.Ok(healthControls);
    }

    /// <summary>
    /// List health controls by health control Id.
    /// </summary>
    /// <param name="controlId">Health control id.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{controlId}")]
    [ProducesResponseType(typeof(HealthControlList), 200)]
    public async Task<IActionResult> GetHealthControlsByControlIdAsync(
        [FromRoute] Guid controlId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IBatchResults<HealthControlModel> healthControlModelResults
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
                .CreateHealthControlCollectionComponent(
                    this.requestHeaderContext.TenantId,
                    this.requestHeaderContext.AccountObjectId)
                .ById(controlId)
                .Get(cancellationToken);

        var healthControls = new HealthControlList
        {
            Value = healthControlModelResults.Results.Select(
            healthControl => this.adapterRegistry.AdapterFor<HealthControlModel, HealthControl>()
            .FromModel(healthControl))
            .ToList()
        };

        return this.Ok(healthControls);
    }
}

