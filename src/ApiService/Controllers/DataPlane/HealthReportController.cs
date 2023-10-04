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
/// Health Reports controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/reports")]
public class HealthReportController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly ModelAdapterRegistry adapterRegistry;

    /// <summary>
    /// Health Report controller constructor
    /// </summary>
    public HealthReportController(
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
    /// Get a health report by id.
    /// </summary>
    /// <param name="reportId">id of the health report.</param>
    /// <param name="apiVersion">The api version of the call.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpGet]
    [Route("{reportId}")]
    [ProducesResponseType(typeof(HealthReport), 200)]
    public async Task<IActionResult> GetHealthReportAsync(
    [FromRoute] Guid reportId,
    [FromQuery(Name = "api-version")] string apiVersion,
    CancellationToken cancellationToken)
    {
        IHealthReportModel<HealthReportProperties> healthReportModel
            = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthReportCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .ById(reportId)
        .Get(cancellationToken);

        return this.Ok(
            this.adapterRegistry.AdapterFor<IHealthReportModel<HealthReportProperties>, HealthReport>(
                healthReportModel.Properties.Id)
            .FromModel(healthReportModel));
    }

    /// <summary>
    /// List all health reports.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="skipToken">The continuation token to list the next page.</param>
    /// <param name="reportKind">Report kind.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(HealthReportList), 200)]
    public async Task<IActionResult> ListHealthReportsAsync(
        [FromQuery(Name = "reportKind")] HealthReportKind reportKind,
        [FromQuery(Name = "api-version")] string apiVersion,
        [FromQuery(Name = "skipToken")] string skipToken = null)
    {
        IBatchResults<IHealthReportModel<HealthReportProperties>> results = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateHealthReportCollectionComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId)
            .Get(skipToken, reportKind);

        var healthReports = new HealthReportList
        {
            Value = results.Results.Select(
            healthReport => this.adapterRegistry.AdapterFor<IHealthReportModel<HealthReportProperties>, HealthReport>()
            .FromModel(healthReport))
            .ToList()
        };

        return this.Ok(healthReports);
    }
}
