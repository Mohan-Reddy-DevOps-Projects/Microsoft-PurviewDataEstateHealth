// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Storage.Queues.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

internal class DEHRunScheduleStage : IJobCallbackStage
{
    private const int GetMessageSize = 32;
    private const int MessageConcurrencyCount = 5;
    private const int MaxRetryCount = 24 * 3; // 3 days

    private readonly JobCallbackUtils<DEHTriggeredScheduleJobMetadata> jobCallbackUtils;

    private readonly DEHTriggeredScheduleJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly TriggeredScheduleQueue triggeredScheduleQueue;

    private readonly IDataHealthApiService dataHealthApiService;

    private readonly CancellationToken cancellationToken;

    public DEHRunScheduleStage(
        IServiceScope scope,
        DEHTriggeredScheduleJobMetadata metadata,
        JobCallbackUtils<DEHTriggeredScheduleJobMetadata> jobCallbackUtils,
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
        var count = this.triggeredScheduleQueue.GetApproximateMessagesCount();
        this.logger.LogTipInformation("Start to execute DEHRunScheduleStage.", new JObject { { "scheduleCountInQueue", count } });
        var messages = await this.triggeredScheduleQueue.ReceiveMessagesAsync(GetMessageSize, TimeSpan.FromHours(1)).ConfigureAwait(false);
        this.logger.LogTipInformation("Retrieved triggered schedules", new JObject { { "retrievedScheduleCount", messages.Length } });
        try
        {
            await this.ParallelTriggerDEHSchedule(messages, MessageConcurrencyCount).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Error occurred while processing triggered schedules", ex);
            return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, $"Exception happens in schedule trigger. {ex.Message}");
        }

        return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, "All retrieved schedules are successfully triggered.");
    }

    private async Task ParallelTriggerDEHSchedule(QueueMessage[] messages, int concurrencyCount)
    {
        var tasks = new List<Task>();
        var count = messages.Length;
        for (var i = 0; i < count || tasks.Count > 0; ++i)
        {
            this.cancellationToken.ThrowIfCancellationRequested();
            if (i < count)
            {
                tasks.Add(this.TriggerDEHSchedule(messages[i]));
                this.logger.LogInformation($"New DEH schedule added in parallel tasks. Index: {i}. Message Id: {messages[i].MessageId}. {messages[i].Body}.");
            }
            // Wait in 2 cases.
            // Case 1: The number of tasks reaches the concurrency count.
            // Case 2: The number of tasks reaches the end of the messages.
            if (tasks.Count >= concurrencyCount || i >= count)
            {
                try
                {
                    await Task.WhenAny(tasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Error occurred while processing triggered schedules", ex);
                }
                foreach (var task in tasks.Where(t => t.IsCompleted || t.IsCanceled || t.IsFaulted))
                {
                    this.logger.LogInformation($"DEH schedule done and removed from tasks list. Task status: {task.Status}. Task id: {task.Id}.");
                }
                tasks = tasks.Where(t => !t.IsCompleted && !t.IsCanceled && !t.IsFaulted).ToList();
            }
        }
    }

    private async Task TriggerDEHSchedule(QueueMessage message)
    {
        this.logger.LogInformation($"Trigger DEH schedule. Schedule enqueue time: {message.InsertedOn}. Message Id: {message.MessageId}.");
        DHScheduleQueueEntity entity;
        try
        {
            entity = JsonConvert.DeserializeObject<DHScheduleQueueEntity>(message.MessageText);
        }
        catch (Exception ex)
        {
            this.logger.LogError("Failed to deserialize DEH schedule message from queue", ex);
            return;
        }
        if (entity == null)
        {
            return;
        }

        if (entity.TryCount >= MaxRetryCount)
        {
            this.logger.LogInformation($"Schedule is outdated. Delete it from queue. AccountId: {entity.AccountId}. ControlId: {entity.ControlId}. TryCount: {entity.TryCount}.");
            await this.triggeredScheduleQueue.DeleteMessage(message.MessageId, message.PopReceipt).ConfigureAwait(false);
            this.logger.LogInformation($"Outdated scheduled is successfully delete from queue. AccountId: {entity.AccountId}. ControlId: {entity.ControlId}. TryCount: {entity.TryCount}.");
            return;
        }

        var payload = new TriggeredSchedulePayload()
        {
            TenantId = entity.TenantId,
            AccountId = entity.AccountId,
            ControlId = entity.ControlId,
            Operator = entity.Operator,
            TriggerType = entity.TriggerType.ToString(),
            RequestId = Guid.NewGuid().ToString(),
        };
        var result = await this.dataHealthApiService.TriggerDEHSchedule(payload, this.cancellationToken).ConfigureAwait(false);
        if (result)
        {
            await this.triggeredScheduleQueue.DeleteMessage(message.MessageId, message.PopReceipt).ConfigureAwait(false);
            this.logger.LogInformation($"Deleted successfully triggered schedule in queue. AccountId: {entity.AccountId}. ControlId: {entity.ControlId}.");
        }
        else
        {
            entity.TryCount += 1;
            var body = JsonConvert.SerializeObject(entity);
            await this.triggeredScheduleQueue.UpdateMessage(message.MessageId, message.PopReceipt, body).ConfigureAwait(false);
            this.logger.LogInformation($"Fail to trigger schedule. Update queue message. Try count: {entity.TryCount}. AccountId: {entity.AccountId}. ControlId: {entity.ControlId}.");
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
