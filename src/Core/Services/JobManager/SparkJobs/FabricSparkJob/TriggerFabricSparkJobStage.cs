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
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;

    private readonly DataPlaneSparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IFabricSparkJobComponent fabricSparkJobComponent;

    public TriggerFabricSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
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
        var jobId = Guid.NewGuid().ToString();


        try
        {
            var jobInfo = await this.fabricSparkJobComponent.SubmitJob(
                this.metadata.AccountServiceModel,
                new CancellationToken(),
                jobId,
                this.metadata.SparkPoolId);
            if (jobInfo == null)
            {
                this.dataEstateHealthRequestLogger.LogInformation($"Copy Activity not configured account: {this.metadata.AccountServiceModel.Id} in {this.StageName}");
                jobStatusMessage = $"Copy Activity not configured account: {this.metadata.AccountServiceModel.Id} in {this.StageName}";
                jobStageStatus = JobExecutionStatus.Succeeded;                
                this.metadata.FabricSparkJobBatchId = "-1";
                return this.jobCallbackUtils.GetExecutionResult(JobExecutionStatus.Completed, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
            }

            this.metadata.SparkPoolId = jobInfo.PoolResourceId;
            this.metadata.FabricSparkJobBatchId = jobInfo.JobId;

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
        return int.TryParse(this.metadata.FabricSparkJobBatchId, out int _) && this.metadata.FabricSparkJobStatus != DataPlaneSparkJobStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return string.IsNullOrEmpty(this.metadata.FabricSparkJobBatchId);
    }
}
