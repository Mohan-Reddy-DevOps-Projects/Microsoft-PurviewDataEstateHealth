// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TriggerDimensionModelSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;

    private readonly DataPlaneSparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IDimensionModelSparkJobComponent dimensionModelSparkJobComponent;

    public TriggerDimensionModelSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
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
        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger dimension spark job"))
        {
            try
            {
                var jobInfo = await this.dimensionModelSparkJobComponent.SubmitJob(
                    this.metadata.AccountServiceModel,
                    new CancellationToken(),
                    jobId,
                    this.metadata.SparkPoolId);

                this.metadata.SparkPoolId = jobInfo.PoolResourceId;
                this.metadata.DimensionSparkJobBatchId = jobInfo.JobId;

                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"DEH_Dimentional_Model job submitted for account: {this.metadata.AccountServiceModel.Id} in {this.StageName}, JobID : {jobId}, poolId : {jobInfo.PoolResourceId}, SparkJobBatchId : {jobInfo.JobId} ";
                this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"Failed to submit DEH_Dimentional_Model job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}, JobID : {jobId} ";
                this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        return int.TryParse(this.metadata.DimensionSparkJobBatchId, out int _);
    }

    public bool IsStagePreconditionMet()
    {
        return string.IsNullOrEmpty(this.metadata.DimensionSparkJobBatchId) &&
            this.metadata.CatalogSparkJobStatus == DataPlaneSparkJobStatus.Succeeded;
    }
}
