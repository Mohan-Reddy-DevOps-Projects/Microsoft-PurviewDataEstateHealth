// <copyright file="DHAssessmentController.cs" company="Microsoft Corporation">
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
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/assessments")]
public class DHAssessmentController(DHAssessmentService assessmentService, DHProvisionService provisionService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListAssessmentsAsync()
    {
        var batchResults = await assessmentService.ListAssessmentsAsync().ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateAssessmentAsync([FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHAssessmentWrapper.Create(payload);
        var result = await assessmentService.CreateAssessmentAsync(entity).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{result.Id}"), result.JObject);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetAssessmentById(string id)
    {
        var entity = await assessmentService.GetAssessmentByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> UpdateAssessmentByIdAsync(string id, [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHAssessmentWrapper.Create(payload!);
        var result = await assessmentService.UpdateAssessmentByIdAsync(id, entity).ConfigureAwait(false);
        return this.Ok(result.JObject);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteAssessmentByIdAsync(string id)
    {
        await assessmentService.DeleteAssessmentByIdAsync(id).ConfigureAwait(false);
        return this.NoContent();
    }

    [HttpPost]
    [Route("{id}/reset")]
    public async Task<ActionResult> ResetAssessmentByIdAsync(string id)
    {
        var entity = await provisionService.ResetAssessmentByIdAsync(id).ConfigureAwait(false);

        return this.Ok(entity.JObject);
    }
}
