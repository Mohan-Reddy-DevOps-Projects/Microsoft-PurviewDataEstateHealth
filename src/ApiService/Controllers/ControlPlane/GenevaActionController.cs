// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Geneva action controller.
/// </summary>
[ApiController]
[Route("/controlplane/genevaAction/")]
[ApiVersionNeutral]
public class GenevaActionController : ControlPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;
    private readonly IDataEstateHealthRequestLogger logger;

    public GenevaActionController(
        ICoreLayerFactory coreLayerFactory,
        IDataEstateHealthRequestLogger logger)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Trigger a background job registered in Azure stack.
    /// </summary>
    /// <param name="payload"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("triggerBackgroundJob")]
    public async Task<IActionResult> TriggerBackgroundJobAsync(
        [FromBody] GenevaActionTriggerBackgroundJobRequestPayload payload,
        CancellationToken cancellationToken)
    {
        using (this.logger.LogElapsed($"Geneva action: Trigger background job. {payload.JobPartition} {payload.JobId}"))
        {
            try
            {
                await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreateDHWorkerServiceTriggerComponent(Guid.Empty, Guid.Empty)
                    .TriggerBackgroundJob(payload.JobPartition, payload.JobId, cancellationToken);
                return this.Ok();
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fail to trigger background job. {payload.JobPartition} {payload.JobId}", ex);
                var response = new GenevaActionResponse { Code = "500", Message = ex.Message };
                return this.StatusCode(500, response);
            }
        }
    }


    /// <summary>
    /// Get detail of a background job registered in Azure stack.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("getBackgroundJobDetail")]
    public async Task<IActionResult> GetBackgroundJobAsync(
        [FromBody] GenevaActionGetBackgroundJobDetailRequestPayload payload)
    {
        using (this.logger.LogElapsed($"Geneva action: Get background job detail. {payload.JobPartition} {payload.JobId}"))
        {
            try
            {
                var job = await this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreateDHWorkerServiceTriggerComponent(Guid.Empty, Guid.Empty)
                    .GetBackgroundJob(payload.JobPartition, payload.JobId);
                return this.Ok(job);
            }
            catch (Exception ex)
            {
                this.logger.LogCritical($"Fail to get background job detail. {payload.JobPartition} {payload.JobId}", ex);
                var response = new GenevaActionResponse { Code = "500", Message = ex.Message };
                return this.StatusCode(500, response);
            }
        }
    }
}

