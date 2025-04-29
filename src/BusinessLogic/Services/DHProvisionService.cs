namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHProvisionService(
    DHControlService controlService,
    DHDataEstateHealthService dHDataEstateHealthService,
    DHAssessmentService assessmentService,
    DHStatusPaletteService statusPaletteService,
    DHActionService actionService,
    DHScoreService scoreService,
    DHScheduleInternalService scheduleInternalService,
    DHScheduleService scheduleService,
    DHAlertService alertService,
    DHTemplateService templateService,
    IRequestHeaderContext requestHeaderContext,
    IDataEstateHealthRequestLogger logger
    )
{
    public async Task ProvisionAccount(Guid tenantId, Guid accountId)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ProvisionAccount)}"))
        {
            try
            {
                requestHeaderContext.TenantId = tenantId;
                requestHeaderContext.AccountObjectId = accountId;

                var allControls = await controlService.ListControlsAsync().ConfigureAwait(false);

                var controlTemplates = new List<string>() { SystemTemplateNames.CDMC.ToString() };

                foreach (var template in controlTemplates)
                {
                    var templateProvisioned = allControls.Results.Any(x => x.SystemTemplate == template);

                    if (!templateProvisioned)
                    {
                        await templateService.ProvisionControlTemplate(template).ConfigureAwait(false);

                        logger.LogInformation($"Template {template} provisioned");
                    }
                    else
                    {
                        logger.LogInformation($"Template {template} already provisioned, skip for provision.");
                    }
                }

                // Check if global schedule exists before creating a new one
                var globalScheduleExists = await scheduleService.GlobalScheduleExistsAsync().ConfigureAwait(false);
                if (!globalScheduleExists)
                {
                    logger.LogInformation($"Creating global schedule in provision for account {accountId}");
                    await scheduleService.CreateGlobalScheduleInProvision().ConfigureAwait(false);
                }
                else
                {
                    logger.LogInformation($"Global schedule already exists for account {accountId}, skipping creation.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in {this.GetType().Name}#{nameof(ProvisionAccount)}", ex);
                throw;
            }
        }
    }

    public async Task DeprovisionDEHResources()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionDEHResources)}"))
        {
            List<Task> tasks = [
                scheduleInternalService.DeprovisionForSchedulesAsync(),
                controlService.DeprovisionForControlsAsync(),
                assessmentService.DeprovisionForAssessmentsAsync(),
                statusPaletteService.DeprovisionForStatusPalettesAsync(),
                actionService.DeprovisionForActionsAsync(),
                scoreService.DeprovisionForScoresAsync(),
                alertService.DeprovisionForAlertsAsync()
            ];

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }


    public async Task DeprovisionDataPlaneResources()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionDataPlaneResources)}"))
        {
            try
            {
                List<Task> tasks = [
                    dHDataEstateHealthService.DeprovisionForDEHAsync()
                ];
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error in {this.GetType().Name}#{nameof(DeprovisionDataPlaneResources)}", ex);
                throw;
            }
        }
    }

    public async Task DeprovisionDEHResources(Guid tenantId, Guid accountId)
    {
        requestHeaderContext.TenantId = tenantId;
        requestHeaderContext.AccountObjectId = accountId;

        await this.DeprovisionDEHResources().ConfigureAwait(false);
    }

    public async Task DeprovisionDataPlaneResources(Guid tenantId, Guid accountId)
    {
        requestHeaderContext.TenantId = tenantId;
        requestHeaderContext.AccountObjectId = accountId;

        await this.DeprovisionDataPlaneResources().ConfigureAwait(false);
    }

}