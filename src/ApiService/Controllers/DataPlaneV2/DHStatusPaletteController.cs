// <copyright file="DHStatusPaletteController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/controls/statusPalette")]
public class DHStatusPaletteController(DHStatusPaletteService statusPaletteService) : DataPlaneController
{
    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListStatusPalettesAsync()
    {
        var batchResults = await statusPaletteService.ListStatusPalettesAsync().ConfigureAwait(false);
        return this.Ok(PagedResults.FromBatchResults(batchResults));
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateStatusPaletteAsync(
    [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHControlStatusPaletteWrapper.Create(payload);
        var result = await statusPaletteService.CreateStatusPaletteAsync(entity).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}/{result.Id}"), result.JObject);
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult> GetStatusPaletteById(string id)
    {
        var entity = await statusPaletteService.GetStatusPaletteByIdAsync(id).ConfigureAwait(false);
        return this.Ok(entity.JObject);
    }

    [HttpPut]
    [Route("{id}")]
    public async Task<ActionResult> UpdateStatusPaletteByIdAsync(string id, [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var wrapper = DHControlStatusPaletteWrapper.Create(payload);

        var entity = await statusPaletteService.UpdateStatusPaletteByIdAsync(id, wrapper).ConfigureAwait(false);

        return this.Ok(entity.JObject);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<ActionResult> DeleteStatusPaletteByIdAsync(string id)
    {
        await statusPaletteService.DeleteStatusPaletteByIdAsync(id).ConfigureAwait(false);

        return this.NoContent();
    }
}
