// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService.Configurations;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;

/// <summary>
/// Notify partner services
/// </summary>
public static class PartnerNotifier
{
    /// <summary>
    /// Notifies all Partners based on the Account sku.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="partnerService">The partner service.</param>
    /// <param name="partnerConfig">The partner configuration.</param>
    /// <param name="serviceModel">The account service model.</param>
    /// <param name="operationType">The type of the operation.</param>
    /// <param name="partnerContext">Context about each partner.</param>
    /// <returns>Async task.</returns>
    public static async Task NotifyPartners(
        IDataEstateHealthRequestLogger logger,
        IPartnerService<AccountServiceModel, IPartnerDetails> partnerService,
        PartnerConfig<IPartnerDetails> partnerConfig,
        AccountServiceModel serviceModel,
        OperationType operationType,
        ConcurrentDictionary<string, PartnerOptions> partnerContext = null)
    {
        if ((serviceModel?.Sku?.Name) == AccountSkuName.Free)
        {
            return;
        }
        await NotifyPartnersHelper(
            logger,
            partnerService,
            partnerConfig.Partners,
            serviceModel,
            operationType,
            partnerContext).ConfigureAwait(false);
    }

    /// <summary>
    /// Notify the input partners of the updated Account model.
    /// Based on the configuration partners may be notfied in parallel.
    /// Partner Context may be provided to filter out partners that have 
    /// already been successfully notified.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="partnerService">The partner service.</param>
    /// <param name="partners">The list of partners to notify.</param>
    /// <param name="serviceModel">The account service model to notify partners.</param>
    /// <param name="operationType">The type of account operation to notify.</param>
    /// <param name="partnerContext">The existing context fo partners already notified.</param>
    /// <returns>Async task.</returns>
    private static async Task NotifyPartnersHelper(
        IDataEstateHealthRequestLogger logger,
        IPartnerService<AccountServiceModel, IPartnerDetails> partnerService,
        IPartnerDetails[] partners,
        AccountServiceModel serviceModel,
        OperationType operationType,
        ConcurrentDictionary<string, PartnerOptions> partnerContext = null)
    {
        Dictionary<string, IPartnerDetails> tasks = new();
        Dictionary<string, List<string>> dependencies = new();
        PartnerOptions succeededPartnerOption = new() { HasSucceeded = true };

        Action<string> onSuccess = partnerContext == null ? null : (partnerName) =>
            // Using an AddOrUpdate here because we saw a race condition where a new partner is added after the partnerContext has been initialized.
            // This will now catch that case and if any new partners are encountered at this stage, will add them in with the HasSucceeded set to true.
            partnerContext.AddOrUpdate(partnerName.ToLowerInvariant(), succeededPartnerOption, (k, v) =>
            {
                v.HasSucceeded = true;
                return v;
            });

        if (partnerContext != null)
        {
            logger.LogInformation($"Partner Context value is: {partnerContext.ToJson()}");
        }

        foreach (IPartnerDetails partner in partners)
        {
            string partnerName = partner.Name.ToLowerInvariant();
            // Skip partners if and only if they are in the partner context and their HasSucceeded value is true.
            if (partnerContext == null ||
                !partnerContext.TryGetValue(partnerName, out PartnerOptions context) ||
                !context.HasSucceeded)
            {
                tasks.Add(partnerName, partner);
                List<string> dependsOnList = new(partner.DependsOn.ToList());
                dependsOnList.RemoveAll(name => partnerContext != null && partnerContext.TryGetValue(name.ToLowerInvariant(), out context) && context.HasSucceeded);
                dependencies.Add(partnerName, dependsOnList);
            }
            else
            {
                logger.LogInformation($"HasSucceeded value was true for {partner.Name}. Skipping notification for {partner.Name}.");
            }
        }

        List<List<string>> executions = dependencies.ResolveDependencies();
        foreach (List<string> execution in executions)
        {
            logger.LogInformation($"Starting parallel execution for {execution.Count} partners.");
            List<Task> independentTasks = new();
            foreach (string partner in execution)
            {
                logger.LogInformation($"Calling Partner service {partner}.");
                independentTasks.Add(GetPartnerTask(partnerService, partner, serviceModel, tasks, operationType, onSuccess));
            }

            // This calls independent partner services in parallel.
            await Task.WhenAll(independentTasks).ConfigureAwait(false);
            logger.LogInformation($"Completed parallel execution for {execution.Count} partners.");
        }

        logger.LogInformation("Completed Partner service operations.");
    }

    private static Task GetPartnerTask(
        IPartnerService<AccountServiceModel, IPartnerDetails> partnerService,
        string partner,
        AccountServiceModel serviceModel,
        Dictionary<string, IPartnerDetails> tasks,
        OperationType operationType,
        Action<string> onSuccess = null)
    {
        Task partnerTask = operationType switch
        {
            OperationType.CreateOrUpdate => partnerService.CreateOrUpdate(tasks[partner], serviceModel, onSuccess),
            OperationType.Delete => partnerService.Delete(tasks[partner], serviceModel, onSuccess),
            OperationType.SoftDelete => partnerService.Delete(tasks[partner], serviceModel, onSuccess, OperationType.SoftDelete),
            _ => throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture, "Invalid operation type.")),
        };
        return partnerTask;
    }
}
