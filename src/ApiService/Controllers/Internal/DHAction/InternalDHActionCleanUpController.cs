// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService.Controllers.Internal.DHAction;

using Asp.Versioning;
using AspNetCore.Authentication.Certificate;
using IdentityModel.S2S.Extensions.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using System.Threading.Tasks;

/// <summary>
/// Action intenal api controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV2)]
[CertificateConfig(CertificateSet.DHControlSchedule)]
[Authorize(
    Policy = "ClientCertOnly",
    AuthenticationSchemes = S2SAuthenticationDefaults.AuthenticationScheme + "," + CertificateAuthenticationDefaults.AuthenticationScheme
)]
[Route("/internal/actions")]
public class InternalDHActionCleanUpController(DHActionService actionService) : Controller
{
    [HttpDelete]
    [Route("cleanup")]
    public async Task<ActionResult> CleanupActionsAsync()
    {
        await actionService.CleanUpActionsAsync().ConfigureAwait(false);
        return this.Ok();
    }
}
