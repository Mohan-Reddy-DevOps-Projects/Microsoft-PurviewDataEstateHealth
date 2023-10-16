// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using global::Microsoft.AspNetCore.Mvc;
using global::Microsoft.Azure.Purview.DataEstateHealth.Core;
using global::Microsoft.Azure.Purview.DataEstateHealth.Models;
using global::Microsoft.DGP.ServiceBasics.Adapters;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.BaseModels;

/// <summary>
/// Business domains controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/businessDomains/")]
public class BusinessDomainsController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Business domains controller constructor
    /// </summary>
    public BusinessDomainsController(
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
    /// Get all business domains.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <param name="skipToken">The skip token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(BusinessDomainList), 200)]
    public async Task<IActionResult> GetBusinessDomainsAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
         [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IBusinessDomainModel> results = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateBusinessDomainCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(cancellationToken, skipToken);

        var businessDomains = new BusinessDomainList
        {
            Value = results.Results.Select(
            businessDomain => this.adapterRegistry.AdapterFor<IBusinessDomainModel, BusinessDomain>()
            .FromModel(businessDomain))
            .ToList()
        };

        return this.Ok(businessDomains);
    }
}
