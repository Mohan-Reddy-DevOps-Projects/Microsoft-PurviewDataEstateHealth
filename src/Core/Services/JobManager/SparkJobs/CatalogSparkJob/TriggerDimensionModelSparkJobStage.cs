// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Microsoft.Purview.ArtifactStoreClient.Models;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TriggerDimensionModelSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;

    private readonly DataPlaneSparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IDimensionModelSparkJobComponent dimensionModelSparkJobComponent;
    
    private readonly IAccountExposureControlConfigProvider exposureControl;

    public TriggerDimensionModelSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.dimensionModelSparkJobComponent = scope.ServiceProvider.GetService<IDimensionModelSparkJobComponent>();
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
    }

    public string StageName => nameof(TriggerDimensionModelSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        var jobId = Guid.NewGuid().ToString();
        var accountId = this.metadata.AccountServiceModel.Id;
        var tenantId = this.metadata.AccountServiceModel.TenantId;
        var isDEHDataCleanup = this.exposureControl.IsDEHDataCleanup(accountId, string.Empty, tenantId);
        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger dimension spark job"))
        {
            try
            {
                this.metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Others;

                var jobInfo = await this.dimensionModelSparkJobComponent.SubmitJob(
                    this.metadata.AccountServiceModel,
                    new CancellationToken(),
                    jobId,
                    this.metadata.SparkPoolId,
                    isDEHDataCleanup,
                    this.metadata.JobRunId);

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
        // if the job is failed, we need to retry the job
        return int.TryParse(this.metadata.DimensionSparkJobBatchId, out int _) && this.metadata.DimensionSparkJobStatus != DataPlaneSparkJobStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return this.metadata.CatalogSparkJobStatus == DataPlaneSparkJobStatus.Succeeded &&
            (string.IsNullOrEmpty(this.metadata.DimensionSparkJobBatchId) || this.metadata.DimensionSparkJobStatus == DataPlaneSparkJobStatus.Failed);
    }
}
