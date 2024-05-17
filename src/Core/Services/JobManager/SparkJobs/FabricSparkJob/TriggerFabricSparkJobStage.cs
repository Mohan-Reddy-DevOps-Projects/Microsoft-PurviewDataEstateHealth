// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TriggerFabricSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<SparkJobMetadata> jobCallbackUtils;

    private readonly SparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IFabricSparkJobComponent fabricSparkJobComponent;

    public TriggerFabricSparkJobStage(
    IServiceScope scope,
    SparkJobMetadata metadata,
    JobCallbackUtils<SparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.fabricSparkJobComponent = scope.ServiceProvider.GetService<IFabricSparkJobComponent>();
    }

    public string StageName => nameof(TriggerFabricSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;

        try
        {
            var jobInfo = await this.fabricSparkJobComponent.SubmitJob(
                this.metadata.AccountServiceModel,
                new CancellationToken());

            this.metadata.SparkPoolId = jobInfo.PoolResourceId;
            this.metadata.SparkJobBatchId = jobInfo.JobId;

            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"Fabric SPARK job submitted for account: {this.metadata.AccountServiceModel.Id} in {this.StageName}";
            this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"Failed to submit Fabric SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
            this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
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
