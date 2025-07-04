﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using OperationType = DataTransferObjects.OperationType;

/// <summary>
/// Purview account notifications controller.
/// </summary>
[ApiController]
[Route("/controlplane/account/")]
[ApiVersionNeutral]
public class PlatformAccountNotificationsController : ControlPlaneController
{
    private readonly ICoreLayerFactory coreLayerFactory;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IAccountExposureControlConfigProvider exposureControl;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly DHProvisionService dhProvisionService;
    /// <summary>
    /// Instantiate instance of PlatformAccountNotificationsController.
    /// </summary>
    public PlatformAccountNotificationsController(
        ICoreLayerFactory coreLayerFactory,
        IAccountExposureControlConfigProvider exposureControl,
        IDataEstateHealthRequestLogger logger,
        IProcessingStorageManager processingStorageManager,
        DHProvisionService provisionService,
        ControllerContext controllerContext = null)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.logger = logger;
        this.exposureControl = exposureControl;
        this.processingStorageManager = processingStorageManager;
        this.dhProvisionService = provisionService;

        if (controllerContext != null)
        {
            this.ControllerContext = controllerContext;
        }
    }

    /// <summary>
    /// Create or update account notification.
    /// </summary>
    /// <param name="account">The model of the platform account</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPut]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> CreateOrUpdateNotificationAsync(
        [FromBody] AccountServiceModel account,
        CancellationToken cancellationToken)
    {
        try
        {
            List<Task> tasks = [
                this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreatePartnerNotificationComponent(
                    Guid.Parse(account.TenantId),
                    Guid.Parse(account.Id))
                    .CreateOrUpdateNotification(account, cancellationToken),
                this.dhProvisionService.ProvisionAccount(Guid.Parse(account.TenantId), Guid.Parse(account.Id)) // Provision control template
            ];

            await Task.WhenAll(tasks);

            return this.Ok();
        }
        catch (Exception ex)
        {
            this.logger.LogCritical($"Provisioning failed, tenantId: {account.TenantId}", ex);
            ProvisioningFailedResponse response = new ProvisioningFailedResponse { Code = "500", Message = ex.Message };
            return this.StatusCode(500, response);
        }
    }

    /// <summary>
    /// Deletes the account dependencies.
    /// </summary>
    /// <param name="accountId">The accountId of the Service Account</param>
    /// <param name="operation">The type of operation to perform on the account.</param>
    /// <param name="account">The account model.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpDelete]
    [ProducesResponseType(typeof(AccountServiceModel), 200)]
    [ProducesResponseType(204)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("{accountId}/")]
    public async Task<IActionResult> DeleteOrSoftDeleteNotificationAsync(
        [FromRoute] Guid accountId,
        [FromQuery(Name = "operation")] OperationType operation,
        [FromBody] AccountServiceModel account,
        CancellationToken cancellationToken)
    {
        try
        {
            List<Task> tasks = [
                this.dhProvisionService.DeprovisionDEHResources(Guid.Parse(account.TenantId), Guid.Parse(account.Id)),
                this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                    .CreatePartnerNotificationComponent(
                    Guid.Parse(account.TenantId),
                    Guid.Parse(account.Id))
                    .DeleteNotification(account, cancellationToken),
                this.dhProvisionService.DeprovisionDataPlaneResources(Guid.Parse(account.TenantId), Guid.Parse(account.Id))
            ];

            await Task.WhenAll(tasks);

            return this.Ok();
        }
        catch (Exception ex)
        {
            this.logger.LogCritical($"Deprovisioning failed, tenantId: {account.TenantId}", ex);
            ProvisioningFailedResponse response = new ProvisioningFailedResponse { Code = "500", Message = ex.Message };
            return this.StatusCode(500, response);
        }
    }
}

public record ProvisioningFailedResponse
{
    [JsonProperty("code")]
    public required string Code { get; set; }

    [JsonProperty("message")]
    public required string Message { get; set; }
}