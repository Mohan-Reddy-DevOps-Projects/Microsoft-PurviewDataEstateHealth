// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHAction;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

/// <summary>
/// Action intenal api controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DataQuality)]
[Authorize(AuthenticationSchemes = "Certificate")]
[Route("/internal/actions")]
public class InternalDHActionController(DHActionService actionService) : Controller
{
    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateActionAsync(
        [FromBody] JArray payload)
    {
        var actionWrapperList = payload.Select(item => DataHealthActionWrapper.Create((JObject)item)).ToList();

        var entites = await actionService.CreateActionsAsync(actionWrapperList).ConfigureAwait(false);
        return this.Ok(entites.Select((item) => item.JObject));
    }

}
