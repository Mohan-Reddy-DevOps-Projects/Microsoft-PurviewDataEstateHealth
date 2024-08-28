// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.GovernedAssetsJobs;

using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.Metadata;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading.Tasks;

internal class ProcessComputingAssetsSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<GovernedAssetsJobMetadata> jobCallbackUtils;

    private readonly GovernedAssetsJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly IComputeGovernedAssetsSparkJobComponent billingSparkJobComponent;

    public ProcessComputingAssetsSparkJobStage(
        IServiceScope scope,
        GovernedAssetsJobMetadata metadata,
        JobCallbackUtils<GovernedAssetsJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.billingSparkJobComponent = scope.ServiceProvider.GetService<IComputeGovernedAssetsSparkJobComponent>();
    }

    public string StageName => nameof(ProcessComputingAssetsSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        var jobId = Guid.NewGuid().ToString();

        this.metadata.CurrentScheduleStartTime = DateTime.UtcNow;

        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to trigger process governed assets spark job"))
        {
            foreach (var billingAccount in this.metadata.GovernedAssetsJobAccounts)
            {
                if (billingAccount.ComputeGovernedAssetsSparkJobStatus != DataPlaneSparkJobStatus.Succeeded)
                {
                    try
                    {
                        billingAccount.ComputeGovernedAssetsSparkJobStatus = DataPlaneSparkJobStatus.Others;

                        var jobInfo = await this.billingSparkJobComponent.SubmitJob(
                            billingAccount.AccountServiceModel,
                            new CancellationToken(),
                            jobId,
                            "/subscriptions/e1da0443-1042-4b54-ba8c-13849f76bd48/resourceGroups/dgh-dogfood-westus2-rg/providers/Microsoft.Synapse/workspaces/dghdogfoodsynapse/bigDataPools/jartest"
                            );//this.metadata.SparkPoolId

                        this.metadata.SparkPoolId = jobInfo.PoolResourceId;
                        billingAccount.ComputeGovernedAssetsSparkJobBatchId = jobInfo.JobId;
                        // TODO pull
                        billingAccount.ComputeGovernedAssetsSparkJobStatus = DataPlaneSparkJobStatus.Succeeded;

                        var jobStatusMessage = $"DEH_ComputeGovernedAssets job submitted for account: {billingAccount.AccountServiceModel.Id} in {this.StageName}, JobID : {jobId} ";
                        this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);
                    }
                    catch (Exception exception)
                    {
                        var jobStatusMessage = $"Failed to submit DEH_ComputeGovernedAssets job for account: {billingAccount.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}, JobID : {jobId}";
                        this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
                    }
                }
            }

            var jobExecutionStatus = this.IsStageComplete() ? JobExecutionStatus.Succeeded : JobExecutionStatus.Postponed;

            return this.jobCallbackUtils.GetExecutionResult(
                jobExecutionStatus,
                $"{this.StageName} : Status {jobExecutionStatus}",
                DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        return this.metadata.GovernedAssetsJobAccounts.All(billingAccount => billingAccount.ComputeGovernedAssetsSparkJobStatus == DataPlaneSparkJobStatus.Succeeded);
    }

    public bool IsStagePreconditionMet()
    {
        return true;
    }
}
