// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class MigrateScheduleStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;

    private readonly DataPlaneSparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly TriggeredScheduleQueue triggeredScheduleQueue;

    private readonly IDataHealthApiService dataHealthApiService;

    private readonly CancellationToken cancellationToken;

    public MigrateScheduleStage(
        IServiceScope scope,
        DataPlaneSparkJobMetadata metadata,
        JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils,
        CancellationToken cancellationToken)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.triggeredScheduleQueue = scope.ServiceProvider.GetService<TriggeredScheduleQueue>();
        this.dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();
        this.cancellationToken = cancellationToken;
    }

    public string StageName => nameof(DEHRunScheduleStage);

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"Migrate Schedule Stage."))
        {
            try
            {
                var payload = new MigrateSchedulePayload()
                {
                    TenantId = this.metadata.AccountServiceModel.TenantId,
                    AccountId = this.metadata.AccountServiceModel.Id,
                    RequestId = Guid.NewGuid().ToString(),
                };
                await this.dataHealthApiService.MigrateSchedule(payload, this.cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred while processing migrate schedule stage", ex);
                return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Exception happens in migrate schedule. {ex.Message}");
            }
            return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Schedule are successfully migrated.");
        }
    }

    public bool IsStageComplete()
    {
        return false;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
