// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class TriggerComputeGovernedAssetsSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;

    private readonly DataPlaneSparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;
    private readonly IComputeGovernedAssetsSparkJobComponent billingSparkJobComponent;
    private readonly IAccountExposureControlConfigProvider exposureControl;


    public TriggerComputeGovernedAssetsSparkJobStage(
        IServiceScope scope,
        DataPlaneSparkJobMetadata metadata,
        JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.billingSparkJobComponent = scope.ServiceProvider.GetService<IComputeGovernedAssetsSparkJobComponent>();
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
    }

    public string StageName => nameof(TriggerComputeGovernedAssetsSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;
        var jobId = Guid.NewGuid().ToString();

        this.metadata.CurrentScheduleStartTime = DateTime.UtcNow;

        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger catalog spark job"))
        {
            try
            {
                var jobInfo = await this.billingSparkJobComponent.SubmitJob(
                    this.metadata.AccountServiceModel,
                    new CancellationToken(),
                    jobId,
                    this.metadata.SparkPoolId);

                this.metadata.SparkPoolId = jobInfo.PoolResourceId;
                this.metadata.ComputeGovernedAssetsSparkJobBatchId = jobInfo.JobId;

                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"DEH_ComputeGovernedAssets job submitted for account: {this.metadata.AccountServiceModel.Id} in {this.StageName}, JobID : {jobId} ";
                this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
            }
            catch (Exception exception)
            {
                // TODO Not throw error/log error for the time being
                // Fill an id to complete this stage
                this.metadata.ComputeGovernedAssetsSparkJobBatchId = "-1";
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"Failed to submit DEH_ComputeGovernedAssets job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}, JobID : {jobId}";
                this.dataEstateHealthRequestLogger.LogInformation(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        return !this.exposureControl.IsDataGovBillingEventEnabled(this.metadata.AccountServiceModel.Id, this.metadata.AccountServiceModel.SubscriptionId, this.metadata.AccountServiceModel.TenantId)
            || this.metadata.ComputeGovernedAssetsSparkJobBatchId != null;
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
