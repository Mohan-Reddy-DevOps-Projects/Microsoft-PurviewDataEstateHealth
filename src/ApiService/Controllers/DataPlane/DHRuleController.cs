// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;

/// <summary>
/// Health Reports controller.
/// </summary>
[ApiController]
[Route("/controls")]
public class DHRuleController(
    DHRuleService dHRuleService) : DataPlaneController
{
    [HttpPost]
    [Route("simpleRule")]
    public async Task<ActionResult> CreateDHSimpleRuleAsync(
        [FromBody] DHSimpleRule dHSimpleRule)
    {
        await dHRuleService.CreateDHSimpleRule(dHSimpleRule).ConfigureAwait(false);
        return this.Ok();
    }
}
