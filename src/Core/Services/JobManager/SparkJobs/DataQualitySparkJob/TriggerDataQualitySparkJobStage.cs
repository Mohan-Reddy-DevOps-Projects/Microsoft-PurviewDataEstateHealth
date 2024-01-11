// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

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

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;

        try
        {
            this.metadata.SparkJobBatchId = await this.dataQualitySparkJobComponent.SubmitJob(
                this.metadata.AccountServiceModel,
                new CancellationToken());

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
