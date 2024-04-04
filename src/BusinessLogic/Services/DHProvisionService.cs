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
        }
    }

    public async Task DeprovisionAccount()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionAccount)}"))
        {
            List<Task> tasks = [
                controlService.DeprovisionForControlsAsync(),
                assessmentService.DeprovisionForAssessmentsAsync(),
                statusPaletteService.DeprovisionForStatusPalettesAsync(),
                scheduleInternalService.DeprovisionForSchedulesAsync(),
                actionService.DeprovisionForActionsAsync(),
                scoreService.DeprovisionForScoresAsync(),
                alertService.DeprovisionForAlertsAsync()
            ];

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }


    public async Task DeprovisionDEHAccount()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(DeprovisionDEHAccount)}"))
        {
            List<Task> tasks = [
             dHDataEstateHealthService.DeprovisionForDEHAsync()
               ];
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }



    public async Task DeprovisionAccount(Guid tenantId, Guid accountId)
    {
        requestHeaderContext.TenantId = tenantId;
        requestHeaderContext.AccountObjectId = accountId;

        await this.DeprovisionAccount().ConfigureAwait(false);
        await this.DeprovisionDEHAccount().ConfigureAwait(false);
    }

}