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
using Microsoft.PowerBI.Api.Models;
using System.Data;
using System.Threading;

/// <summary>
/// Refresh controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/datasets/{datasetId}/refreshes")]
public class RefreshController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Health Report controller constructor
    /// </summary>
    public RefreshController(
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
    /// List all health reports.
    /// </summary>
    /// <param name="datasetId">The dataset id.</param>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="cancellationToken"></param>
    /// <param name="top">The number of entities to return.</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthReportList), 200)]
    public async Task<IActionResult> ListRefreshesAsync(
        [FromRoute(Name = "datasetId")] Guid datasetId,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken,
        [FromQuery(Name = "top")] int? top = null,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IRefreshHistoryComponent refreshHistoryComponent = this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateRefreshHistoryComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId,
                datasetId,
                top);
        IBatchResults<PowerBI.Api.Models.Refresh> results = await refreshHistoryComponent.Get(cancellationToken, skipToken);

        RefreshList refreshes = new()
        {
            Value = results.Results.Select(
                refresh => refresh.FromModel())
            .ToList()
        };

        return this.Ok(refreshes);
    }
}
