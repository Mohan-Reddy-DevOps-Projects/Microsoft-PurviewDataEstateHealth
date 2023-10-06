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

/// <summary>
/// Purview account notifications controller.
/// </summary>
[ApiController]
[Route("/controlplane/account/{accountId}/")]
[ApiVersionNeutral]
public class PlatformAccountNotificationsController : ControlPlaneController
{
    /// <summary>
    /// Instantiate instance of PlatformAccountNotificationsController.
    /// </summary>
    /// <param name="controllerContext">Controller Context.</param>
    public PlatformAccountNotificationsController(
        ControllerContext controllerContext = null)
    {
        if (controllerContext != null)
        {
            this.ControllerContext = controllerContext;
        }
    }

    /// <summary>
    /// Create or update account notification.
    /// </summary>
    /// <param name="account">The model of the platform account</param>
    /// <returns></returns>
    [HttpPut]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("/controlplane/account/")]
    public async Task<IActionResult> CreateOrUpdateNotificationAsync(
        [FromBody] AccountServiceModel account)
    {
        await Task.CompletedTask;
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
