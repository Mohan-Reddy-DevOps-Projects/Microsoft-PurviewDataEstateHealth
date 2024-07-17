// <copyright file="DHControlController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.Exceptions;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.StorageConfig;
using Newtonsoft.Json.Linq;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/settings")]
public class DHSettingsController(
    DHStorageConfigService dhStorageConfigService
    ) : DataPlaneController
{
    [HttpGet]
    [Route("storageConfig")]
    public async Task<ActionResult> GetStorageConfigAsync()
    {
        var storageConfig = await dhStorageConfigService.GetStorageConfig().ConfigureAwait(false);
        return this.Ok(storageConfig.JObject);
    }

    [HttpPost]
    [Route("storageConfig")]
    public async Task<ActionResult> UpdateStorageConfigAsync(
    [FromBody] JObject payload)
    {
        if (payload == null)
        {
            throw new InvalidRequestException(StringResources.ErrorMessageInvalidPayload);
        }

        var entity = DHStorageConfigBaseWrapper.Create(payload);
        var result = await dhStorageConfigService.UpdateStorageConfig(entity).ConfigureAwait(false);
        return this.Created(new Uri($"{this.Request.GetEncodedUrl()}"), result.JObject);
    }
}
