// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TriggerDataQualitySparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<SparkJobMetadata> jobCallbackUtils;

    private readonly SparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IDataQualitySparkJobComponent dataQualitySparkJobComponent;

    public TriggerDataQualitySparkJobStage(
    IServiceScope scope,
    SparkJobMetadata metadata,
    JobCallbackUtils<SparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.dataQualitySparkJobComponent = scope.ServiceProvider.GetService<IDataQualitySparkJobComponent>();
    }

    public string StageName => nameof(TriggerDataQualitySparkJobStage);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<JobExecutionResult> Execute()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        /*
         * skip DQ spark job
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;

        try
        {
            var jobInfo = await this.dataQualitySparkJobComponent.SubmitJob(
                this.metadata.AccountServiceModel,
                new CancellationToken());

            this.metadata.SparkPoolId = jobInfo.PoolResourceId;
            this.metadata.SparkJobBatchId = jobInfo.JobId;

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"{this.StageName} | Catalog SPARK job submitted for account: {this.metadata.AccountServiceModel.Id}";
            this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"{this.StageName} | Failed to submit Catalog SPARK job for account: {this.metadata.AccountServiceModel.Id} with error: {exception.Message}";
            this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));

        */

        return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, "", DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
    }

    public bool IsStageComplete()
    {
        return int.TryParse(this.metadata.SparkJobBatchId, out int _);
    }

    public bool IsStagePreconditionMet()
    {
        return string.IsNullOrEmpty(this.metadata.SparkJobBatchId);
    }
}
