// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Token controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/token")]
public class TokenController : DataPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    /// <summary>
    /// Token Controller constructor.
    /// </summary>
    public TokenController(
        ICoreLayerFactory coreLayerFactory,
        IRequestHeaderContext requestHeaderContext,
        ControllerContext controllerContext = null)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.requestHeaderContext = requestHeaderContext;

        if (controllerContext != null)
        {
            this.ControllerContext = controllerContext;
        }
    }

    /// <summary>
    /// Generates an embed token for multiple reports, datasets, and target workspaces.
    /// </summary>
    /// <param name="apiVersion">The api version of the call</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmbedToken), 200)]
    public async Task<IActionResult> PostToken(
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        EmbedToken response = await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreateTokenComponent(
                this.requestHeaderContext.TenantId,
                this.requestHeaderContext.AccountObjectId, "health").Get(cancellationToken);

        return this.Ok(response);
    }
}
