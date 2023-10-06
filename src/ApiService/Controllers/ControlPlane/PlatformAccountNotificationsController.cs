// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.Share.ApiService;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.Azure.Purview.DataEstateHealth.ApiService.DataTransferObjects;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Adapters;
using System.Threading;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;

/// <summary>
/// Purview account notifications controller.
/// </summary>
[ApiController]
[Route("/controlplane/account/{accountId}/")]
[ApiVersionNeutral]
public class PlatformAccountNotificationsController : ControlPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;

    private readonly IRequestHeaderContext requestHeaderContext;

    private readonly EnvironmentConfiguration environmentConfiguration;

    /// <summary>
    /// Instantiate instance of PlatformAccountNotificationsController.
    /// </summary>
    public PlatformAccountNotificationsController(
        ICoreLayerFactory coreLayerFactory,
        IRequestHeaderContext requestHeaderContext,
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        ControllerContext controllerContext = null)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.requestHeaderContext = requestHeaderContext;
        this.environmentConfiguration = environmentConfiguration.Value;
        if (controllerContext != null)
        {
            this.ControllerContext = controllerContext;
        }
    }

    /// <summary>
    /// Create or update account notification.
    /// </summary>
    /// <param name="account">The model of the platform account</param>
    /// <param name="apiVersion"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/controlplane/account/")]
    public async Task<IActionResult> CreateOrUpdateNotificationAsync(
        [FromBody] AccountServiceModel account,
        [FromQuery(Name = "api-version")] string apiVersion,
        CancellationToken cancellationToken)
    {
        if (!this.environmentConfiguration.IsDevelopmentEnvironment())
        {
            return this.Ok();
        }

        await this.coreLayerFactory.Of(ServiceVersion.From(apiVersion))
            .CreatePartnerNotificationComponent(
                Guid.Parse(account.TenantId),
                Guid.Parse(account.Id))
            .CreateOrUpdateNotification(account, cancellationToken);

        return this.Ok();
    }

    /// <summary>
    /// Deletes the account dependencies.
    /// </summary>
    /// <param name="accountId">The accountId of the Service Account</param>
    /// <param name="operation">The type of operation to perform on the account.</param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(typeof(AccountServiceModel), 200)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> DeleteOrSoftDeleteNotificationAsync(
        [FromRoute] Guid accountId,
        [FromQuery(Name = "operation")] OperationType operation)
    {
        await Task.CompletedTask;
        return this.Ok();
    }
}
