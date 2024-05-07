// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class DataplaneJobResetStage : IJobCallbackStage
{
    private readonly IDataEstateHealthRequestLogger logger;

    private readonly JobManagementClient jobManagementClient;

    private readonly BackgroundJobResetMetadata metadata;

    private readonly EnvironmentConfiguration environmentConfiguration;

    private readonly JobCallbackUtils<BackgroundJobResetMetadata> jobCallbackUtils;

    public DataplaneJobResetStage(
        IServiceScope scope,
        BackgroundJobResetMetadata metadata,
        JobManagementClient client,
        JobCallbackUtils<BackgroundJobResetMetadata> jobCallbackUtils)
    {
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.jobManagementClient = client;
        this.metadata = metadata;
        this.environmentConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<EnvironmentConfiguration>>().Value;
        this.jobCallbackUtils = jobCallbackUtils;
    }
    public string StageName => nameof(DataplaneJobResetStage);

    public async Task<JobExecutionResult> Execute()
    {
        using (this.logger.LogElapsed($"Reset data plane spark job"))
        {
            try
            {
                var accountId = this.metadata.AccountServiceModel?.Id;
                var accountName = this.metadata.AccountServiceModel?.Name;
                string jobPartition = $"{accountId}-CATALOG-SPARK-JOBS";
                string jobId = $"{accountId}-CATALOG-SPARK-JOB";
                var jobDefinition = await this.jobManagementClient.GetJob(jobPartition, jobId).ConfigureAwait(false);
                if (jobDefinition != null)
                {
                    var catalogRepeatStrategy = this.environmentConfiguration.IsDevelopmentOrDogfoodEnvironment() ?
                        TimeSpan.FromHours(1) : TimeSpan.FromHours(24);
                    this.logger.LogInformation($"Current env, {this.environmentConfiguration.Environment}");
                    this.logger.LogInformation($"Current schedule interval is {catalogRepeatStrategy.Ticks}, flag is {jobDefinition.Flags}, account name: {accountName}");
                    if (jobDefinition.RepeatInterval != catalogRepeatStrategy.Ticks)
                    {
                        this.logger.LogInformation($"Start to reset schedule interval for account, name: {accountName}");
                        JobBuilder jobBuilder = JobBuilder.Create(jobPartition, jobId ?? Guid.NewGuid().ToString())
                            .WithCallback(jobDefinition.Callback)
                            .WithMetadata(jobDefinition.Metadata)
                            .WithStartTime(jobDefinition.StartTime ?? DateTime.UtcNow.AddMinutes(1))
                            .WithRetryStrategy(TimeSpan.FromTicks(jobDefinition.RetryInterval))
                            .WithoutEndTime()
                            .WithRetention(jobDefinition.Retention ?? TimeSpan.FromDays(7))
                            .WithFlags(JobFlags.DeleteJobIfCompleted)
                            .WithRepeatStrategy(catalogRepeatStrategy);
                        await this.jobManagementClient.CreateOrUpdateJob(jobBuilder).ConfigureAwait(false);
                        this.logger.LogInformation($"The schedule interval for the account has been reset, account name: {accountName}");
                    }
                    this.metadata.IsCompleted = true;
                }
                return new JobExecutionResult { Status = JobExecutionStatus.Completed };
            }
            catch (Exception exception)
            {
                var jobStageStatus = JobExecutionStatus.Failed;
                var jobStatusMessage = $"Failed to reset data plane spark job schedule for account: {this.metadata.AccountServiceModel.Id}";
                this.logger.LogCritical(jobStatusMessage, exception);
                return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
            }
        }
    }

    public bool IsStageComplete()
    {
        return this.metadata.IsCompleted;
    }

    public bool IsStagePreconditionMet()
    {
        return !this.metadata.IsCompleted;
    }
}
