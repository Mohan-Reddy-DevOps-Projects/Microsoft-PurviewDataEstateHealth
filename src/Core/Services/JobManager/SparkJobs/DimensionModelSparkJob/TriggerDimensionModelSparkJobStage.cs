// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class TriggerDimensionModelSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<SparkJobMetadata> jobCallbackUtils;

    private readonly SparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IDimensionModelSparkJobComponent dimensionModelSparkJobComponent;

    public TriggerDimensionModelSparkJobStage(
    IServiceScope scope,
    SparkJobMetadata metadata,
    JobCallbackUtils<SparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.dimensionModelSparkJobComponent = scope.ServiceProvider.GetService<IDimensionModelSparkJobComponent>();
    }

    public string StageName => nameof(TriggerDimensionModelSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        var jobId = Guid.NewGuid().ToString();

        try
        {
            this.metadata.SparkJobBatchId = await this.dimensionModelSparkJobComponent.SubmitJob(
                this.metadata.AccountServiceModel,
                new CancellationToken(), jobId);


            jobStageStatus = JobExecutionStatus.Succeeded;
            jobStatusMessage = $"DEH_Dimentional_Model job submitted for account: {this.metadata.AccountServiceModel.Id} in {this.StageName}, JobID : {jobId} ";
            this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"Failed to submit DEH_Dimentional_Model job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}, JobID : {jobId} ";
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
