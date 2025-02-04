// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TriggerCatalogSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;

    private readonly DataPlaneSparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly ICatalogSparkJobComponent catalogSparkJobComponent;

    private readonly IAccountExposureControlConfigProvider exposureControl;

    public TriggerCatalogSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.catalogSparkJobComponent = scope.ServiceProvider.GetService<ICatalogSparkJobComponent>();
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
    }

    public string StageName => nameof(TriggerCatalogSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        var jobId = Guid.NewGuid().ToString();
        var accountId = this.metadata.AccountServiceModel.Id;
        var tenantId = this.metadata.AccountServiceModel.TenantId;
        var isDEHDataCleanup = this.exposureControl.IsDEHDataCleanup(accountId, string.Empty, tenantId);

        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger catalog spark job"))
        {
            try
            {
                this.metadata.CurrentScheduleStartTime = DateTime.UtcNow;
                this.metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Others;

                var jobInfo = await this.catalogSparkJobComponent.SubmitJob(
                    this.metadata.AccountServiceModel,
                    new CancellationToken(),
                    jobId,
                    this.metadata.SparkPoolId,
                    isDEHDataCleanup);

                this.metadata.SparkPoolId = jobInfo.PoolResourceId;
                this.metadata.CatalogSparkJobBatchId = jobInfo.JobId;

                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"DEH_Domain_Model job submitted for account: {this.metadata.AccountServiceModel.Id} in {this.StageName}, JobID : {jobId} ";
                this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"Failed to submit DEH_Domain_Model job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}, JobID : {jobId}";
                this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        // if the job is failed, we need to retry the job
        return int.TryParse(this.metadata.CatalogSparkJobBatchId, out int _) && this.metadata.CatalogSparkJobStatus != DataPlaneSparkJobStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return string.IsNullOrEmpty(this.metadata.CatalogSparkJobBatchId) || this.metadata.CatalogSparkJobStatus == DataPlaneSparkJobStatus.Failed;
    }
}
