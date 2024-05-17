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

internal class DEHScheduleStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DEHScheduleJobMetadata> jobCallbackUtils;

    private readonly DEHScheduleJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly TriggeredScheduleQueue triggeredScheduleQueue;

    private readonly IDataHealthApiService dataHealthApiService;

    private readonly CancellationToken cancellationToken;

    public DEHScheduleStage(
        IServiceScope scope,
        DEHScheduleJobMetadata metadata,
        JobCallbackUtils<DEHScheduleJobMetadata> jobCallbackUtils,
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
        using (this.logger.LogElapsed($"Start DEHScheduleStage. Account ID: {this.metadata.ScheduleAccountId}. Tenant ID: {this.metadata.ScheduleTenantId}."))
        {
            var payload = new TriggeredSchedulePayload()
            {
                TenantId = this.metadata.ScheduleTenantId,
                AccountId = this.metadata.ScheduleAccountId,
                RequestId = Guid.NewGuid().ToString(),
            };
            try
            {
                await this.dataHealthApiService.TriggerDEHScheduleCallback(payload, this.cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred while processing triggered schedules", ex);
                return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Exception happens in schedule trigger. Account ID: {this.metadata.ScheduleAccountId}. Tenant ID: {this.metadata.ScheduleTenantId}.. {ex.Message}");
            }
            return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Schedule callback are successfully triggered. Account ID: {this.metadata.ScheduleAccountId}. Tenant ID: {this.metadata.ScheduleTenantId}.");
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
