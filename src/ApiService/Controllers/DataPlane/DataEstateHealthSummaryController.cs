// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;

/// <summary>
/// Data estate health summary page controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/summary/domains")]
public class DataEstateHealthSummaryController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Data estate health summary page controller constructor
    /// </summary>
    public DataEstateHealthSummaryController(
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
    /// Get summary for all business domains.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(DataEstateHealthSummary), 200)]
    public async Task<IActionResult> GetDataEstateHealthSummaryAsync(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IDataEstateHealthSummaryModel dataEstateHealthSummaryModel
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateDataEstateHealthSummaryComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(cancellationToken);

        return this.Ok(
            this.adapterRegistry.AdapterFor<IDataEstateHealthSummaryModel,DataEstateHealthSummary>()
            .FromModel(dataEstateHealthSummaryModel));
    }

    /// <summary>
    /// Get summary for a chosen business domain.
    /// </summary>
    /// /// <param name="domainId">id of the domain.</param>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(DataEstateHealthSummary), 200)]
    [Route("/domainId")]
    public async Task<IActionResult> GetDataEstateHealthSummaryAsync(
        [FromRoute] Guid domainId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        IDataEstateHealthSummaryModel dataEstateHealthSummaryModel
           = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
           .CreateDataEstateHealthSummaryComponent(
               this.requestHeaderContext.TenantId,
               this.requestHeaderContext.AccountObjectId)
           .ById(domainId)
           .Get(cancellationToken);

        return this.Ok(
                   this.adapterRegistry.AdapterFor<IDataEstateHealthSummaryModel, DataEstateHealthSummary>()
                   .FromModel(dataEstateHealthSummaryModel));
    }
}
