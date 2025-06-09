// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json;
using System.Threading.Tasks;

internal class DehScheduleStage(
    IServiceScope scope,
    DehScheduleJobMetadata metadata,
    JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils,
    CancellationToken cancellationToken)
    : IJobCallbackStage
{
    private readonly IDataEstateHealthRequestLogger logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();

    private readonly TriggeredScheduleQueue triggeredScheduleQueue = scope.ServiceProvider.GetService<TriggeredScheduleQueue>();

    private readonly IDataHealthApiService dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();

    private readonly CancellationToken cancellationToken = cancellationToken;

    public string StageName => nameof(DEHRunScheduleStage);

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"Start DehScheduleStage. Account ID: {metadata.ScheduleAccountId}. Tenant ID: {metadata.ScheduleTenantId}."))
        {
            var entity = new DHScheduleQueueEntity
            {
                TenantId = metadata.ScheduleTenantId,
                AccountId = metadata.ScheduleAccountId,
                Operator = DHScheduleCallbackPayload.DGScheduleServiceOperatorName,
                TriggerType = DHScheduleCallbackTriggerType.Schedule.ToString(),
            };
            try
            {
                this.logger.LogInformation($"New schedule entity. {JsonConvert.SerializeObject(entity)}");
                await this.triggeredScheduleQueue.SendMessageAsync(JsonConvert.SerializeObject(entity)).ConfigureAwait(false);
                this.logger.LogInformation("Successfully enqueue schedule.");
            }
            catch (Exception ex)
            {
                this.logger.LogError("Error occurred while processing triggered schedules", ex);
                return jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Exception happens in schedule trigger. Account ID: {metadata.ScheduleAccountId}. Tenant ID: {metadata.ScheduleTenantId}.. {ex.Message}");
            }
            return jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Schedule callback are successfully triggered. Account ID: {metadata.ScheduleAccountId}. Tenant ID: {metadata.ScheduleTenantId}.");
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
