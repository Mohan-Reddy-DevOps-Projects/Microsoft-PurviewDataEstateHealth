// -----------------------------------------------------------
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
using Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;
using Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;
using System;
using System.Collections.Concurrent;
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
    private readonly IPartnerService<AccountServiceModel, IPartnerDetails> partnerService;
    private readonly PartnerConfig<IPartnerDetails> partnerConfig;
    private readonly IAccountExposureControlConfigProvider exposureControl;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly DHProvisionService dhProvisionService;
    /// <summary>
    /// Instantiate instance of PlatformAccountNotificationsController.
    /// </summary>
    public PlatformAccountNotificationsController(
        ICoreLayerFactory coreLayerFactory,
        IPartnerService<AccountServiceModel, IPartnerDetails> partnerService,
        IAccountExposureControlConfigProvider exposureControl,
        IOptions<PartnerConfiguration> partnerConfiguration,
        IDataEstateHealthRequestLogger logger,
        IProcessingStorageManager processingStorageManager,
        DHProvisionService provisionService,
        ControllerContext controllerContext = null)
    {
        this.coreLayerFactory = coreLayerFactory;
        this.logger = logger;
        this.partnerService = partnerService;
        this.exposureControl = exposureControl;
        this.partnerConfig = new(partnerConfiguration);
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
        if (!this.exposureControl.IsDataGovProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            return this.Ok();
        }

        if (this.exposureControl.IsDataGovProvisioningServiceEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            return this.Ok();
        }

        await this.processingStorageManager.Provision(account, cancellationToken);
        Task partnerTask = PartnerNotifier.NotifyPartners(
                this.logger,
                this.partnerService,
                this.partnerConfig,
                account,
                ProvisioningService.OperationType.CreateOrUpdate,
                InitPartnerContext(this.partnerConfig.Partners));

        List<Task> tasks = new()
        {
            partnerTask
        };

        if (this.exposureControl.IsDataGovHealthProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            Task healthTask = this.coreLayerFactory.Of(ServiceVersion.From(ServiceVersion.V1))
                .CreatePartnerNotificationComponent(
                Guid.Parse(account.TenantId),
                Guid.Parse(account.Id))
            .CreateOrUpdateNotification(account, cancellationToken);
            tasks.Add(healthTask);
        }

        if (this.exposureControl.IsDGDataHealthEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            // Provision control template
            Task provisionControlTemplate = this.dhProvisionService.ProvisionAccount(Guid.Parse(account.TenantId), Guid.Parse(account.Id));
            tasks.Add(provisionControlTemplate);
        }

        await Task.WhenAll(tasks);

        return this.Ok();
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
        if (!this.exposureControl.IsDataGovProvisioningEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            return this.Ok();
        }

        if (this.exposureControl.IsDataGovProvisioningServiceEnabled(account.Id, account.SubscriptionId, account.TenantId))
        {
            return this.Ok();
        }

        await this.processingStorageManager.Delete(account, cancellationToken);

        await PartnerNotifier.NotifyPartners(
                this.logger,
                this.partnerService,
                this.partnerConfig,
                account,
                ProvisioningService.OperationType.Delete,
                InitPartnerContext(this.partnerConfig.Partners)).ConfigureAwait(false);

        return this.Ok();
    }

    /// <summary>
    /// Initializes the Partner Context for Callbacks.
    /// </summary>
    /// <returns>The PartnerContext.</returns>
    private static ConcurrentDictionary<string, PartnerOptions> InitPartnerContext(IPartnerDetails[] partners)
    {
        ConcurrentDictionary<string, PartnerOptions> partnerContext = new();
        foreach (IPartnerDetails partner in partners)
        {
            partnerContext.TryAdd(partner.Name.ToLowerInvariant(), new PartnerOptions() { HasSucceeded = false });
        }

        return partnerContext;
    }
}
