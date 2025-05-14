// <copyright file="DHControlController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls")]
public class DHControlController(
    DHControlService dataHealthControlService,
    DHTemplateService templateService,
    IAccountExposureControlConfigProvider accountExposureControlConfigProvider,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext
    ) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListControlsAsync()
    {
        var batchResults = await dataHealthControlService.ListControlsAsync().ConfigureAwait(false);
        
        // Get account and tenant IDs for exposure control checks
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();
        
        // Check exposure control flags
        bool criticalDataIdentificationEnabled = accountExposureControlConfigProvider.IsDEHCriticalDataIdentificationEnabled(accountId, string.Empty, tenantId);
        bool businessOKRsAlignmentEnabled = accountExposureControlConfigProvider.IsDEHBusinessOKRsAlignmentEnabled(accountId, string.Empty, tenantId);
        
        // Process controls if any flags are enabled
        if (criticalDataIdentificationEnabled || businessOKRsAlignmentEnabled)
        {
            foreach (var control in batchResults.Results)
            {
                this.OverrideControlStatus(control, criticalDataIdentificationEnabled, businessOKRsAlignmentEnabled);
            }
        }
        
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateControlAsync(
    [FromBody] JObject payload,
    [FromQuery(Name = "withNewAssessment")] bool withNewAssessment)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHControlBaseWrapper.Create(payload);
        var result = await dataHealthControlService.CreateControlAsync(entity, withNewAssessment).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{result.Id}"), result.JObject);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetControlById(string id)
    {
        var entity = await dataHealthControlService.GetControlByIdAsync(id).ConfigureAwait(false);
        
        // Get account and tenant IDs for exposure control checks
        var accountId = requestHeaderContext.AccountObjectId.ToString();
        var tenantId = requestHeaderContext.TenantId.ToString();
        
        // Check exposure control flags
        bool criticalDataIdentificationEnabled = accountExposureControlConfigProvider.IsDEHCriticalDataIdentificationEnabled(accountId, string.Empty, tenantId);
        bool businessOKRsAlignmentEnabled = accountExposureControlConfigProvider.IsDEHBusinessOKRsAlignmentEnabled(accountId, string.Empty, tenantId);
        
        // Process control status if any flags are enabled
        if (criticalDataIdentificationEnabled || businessOKRsAlignmentEnabled)
        {
            this.OverrideControlStatus(entity, criticalDataIdentificationEnabled, businessOKRsAlignmentEnabled);
        }
        
        return this.Ok(entity.JObject);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> UpdateControlByIdAsync(string id, [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var wrapper = DHControlBaseWrapper.Create(payload);

        var entity = await dataHealthControlService.UpdateControlByIdAsync(id, wrapper).ConfigureAwait(false);

        return this.Ok(entity.JObject);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteControlByIdAsync(string id, [FromQuery(Name = "deleteAssessment")] bool deleteAssessment)
    {
        await dataHealthControlService.DeleteControlByIdAsync(id, deleteAssessment).ConfigureAwait(false);

        return this.NoContent();
    }

    [HttpPost]
    [Route("{id}/reset")]
    public async Task<ActionResult> ResetControlByIdAsync(string id)
    {
        var entity = await templateService.ResetControlByIdAsync(id).ConfigureAwait(false);

        return this.Ok(entity.JObject);
    }

    [HttpPost]
    [Route("reset")]
    public async Task<ActionResult> ResetAllControlsAsync()
    {
        await templateService.ResetControlTemplate("CDMC").ConfigureAwait(false);

        return this.Ok();
    }
    
    /// <summary>
    /// Overrides a control's status based on exposure control flags
    /// </summary>
    /// <param name="control">The control to process</param>
    /// <param name="criticalDataIdentificationEnabled">Whether the Critical data identification feature is enabled</param>
    /// <param name="businessOKRsAlignmentEnabled">Whether the Business OKRs alignment feature is enabled</param>
    private void OverrideControlStatus(DHControlBaseWrapper control, bool criticalDataIdentificationEnabled, bool businessOKRsAlignmentEnabled)
    {
        // Handle Critical data identification control
        if (criticalDataIdentificationEnabled && 
            string.Equals(control.Name, DHControlConstants.CriticalDataIdentification, StringComparison.OrdinalIgnoreCase) &&
            control.Status == DHControlStatus.InDevelopment)
        {
            control.Status = DHControlStatus.Enabled;
            logger.LogInformation($"Changed Critical data identification control (ID: {control.Id}) status from InDevelopment to Enabled");
        }
        
        // Handle Business OKRs alignment control
        if (businessOKRsAlignmentEnabled && 
            string.Equals(control.Name, DHControlConstants.BusinessOKRsAlignment, StringComparison.OrdinalIgnoreCase) &&
            control.Status == DHControlStatus.InDevelopment)
        {
            control.Status = DHControlStatus.Enabled;
            logger.LogInformation($"Changed Business OKRs alignment control (ID: {control.Id}) status from InDevelopment to Enabled");
        }
    }
}
