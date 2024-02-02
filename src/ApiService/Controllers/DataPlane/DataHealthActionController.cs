// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <summary>
/// Health Reports controller.
/// </summary>
[ApiController]
[ApiVersion(ServiceVersion.LabelV1)]
[Route("/_actions")]
public class DataHealthActionController : DataPlaneController
{
    /*
    private readonly IRequestHeaderContext requestHeaderContext;

    private IDataHealthActionService actionService;

    /// <summary>
    /// Health Report controller constructor
    /// </summary>
    public DataHealthActionController(
        IRequestHeaderContext requestHeaderContext,
        IDataHealthActionService actionService
        )
    {
        this.requestHeaderContext = requestHeaderContext;
        this.actionService = actionService;
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult> ListActionsAsync(
        [FromQuery] string domainId = null)
    {
        //var query = new Query<ObjectiveFilter>()
        //{
        //    Filter = new ObjectiveFilter()
        //    {
        //        DomainIds = string.IsNullOrEmpty(domainId) ? null : new List<string>() { domainId },
        //        Definition = string.IsNullOrEmpty(keyword) ? null : keyword
        //    },
        //};

        await this.actionService.EnumerateActionsAsync().ConfigureAwait(false);

        return this.Ok();
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult> CreateDHSimpleRuleAsync(
        [FromBody] DataHealthActionModel entity)
    {
        await this.actionService.CreateActionsAsync(entity).ConfigureAwait(false);
        return this.Ok();
    }
    */
}
