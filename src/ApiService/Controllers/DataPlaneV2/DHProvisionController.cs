// <copyright file="DHControlController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

#nullable enable
namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.DataPlaneV2;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[Route("/provision")]
public class DHProvisionController(DHProvisionService provisionService) : DataPlaneController
{
    [HttpPost]
    [Route("")]
    public async Task<ActionResult> ProvisionControlsWithTemplateAsync([FromQuery(Name = "templateName")] string templateName)
    {
        await provisionService.ProvisionControlTemplate(templateName).ConfigureAwait(false);
        return this.Ok();
    }

    [HttpPost]
    [Route("validate")]
    public async Task<ActionResult> ValidateControlsWithTemplateAsync([FromQuery(Name = "templateName")] string templateName)
    {
        provisionService.ValidateControlTemplate(templateName);

        await Task.Delay(10).ConfigureAwait(false);
        return this.Ok();
    }

    [HttpPost]
    [Route("cleanup")]
    public async Task<ActionResult> CleanupControlsWithTemplateAsync([FromQuery(Name = "templateName")] string templateName)
    {
        await provisionService.CleanupControlTemplate(templateName).ConfigureAwait(false);

        return this.Ok();
    }

    [HttpPost]
    [Route("deprovision")]
    public async Task<ActionResult> DeprovisionAsync()
    {
        await provisionService.DeprovisionAccount().ConfigureAwait(false);

        return this.Ok();
    }
}
