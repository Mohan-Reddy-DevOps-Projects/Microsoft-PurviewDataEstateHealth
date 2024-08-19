// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

internal class TrackComputeGovernedAssetsSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;
    private readonly DataPlaneSparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IComputeGovernedAssetsSparkJobComponent computeGovernedAssetsSparkJobComponent;
    private readonly IJobManager backgroundJobManager;
    private readonly IDatabaseManagementService databaseManagementService;
    private readonly IAccountExposureControlConfigProvider exposureControl;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackComputeGovernedAssetsSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.computeGovernedAssetsSparkJobComponent = scope.ServiceProvider.GetService<IComputeGovernedAssetsSparkJobComponent>();
        this.backgroundJobManager = scope.ServiceProvider.GetService<IJobManager>();
        this.databaseManagementService = scope.ServiceProvider.GetService<IDatabaseManagementService>();
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
    }

    public string StageName => nameof(TrackComputeGovernedAssetsSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus = JobExecutionStatus.Postponed;
        string jobStatusMessage;
        using (this.logger.LogElapsed($"Start to track compute governed assets spark job"))
        {
            try
            {
                SparkBatchJob jobDetails;

                if (string.IsNullOrEmpty(this.metadata.SparkPoolId))
                {
                    // per-account pre-provisioned pool
                    jobDetails = await this.computeGovernedAssetsSparkJobComponent.GetJob(
                        this.metadata.AccountServiceModel,
                        int.Parse(this.metadata.ComputeGovernedAssetsSparkJobBatchId),
                        new CancellationToken());
                }
                else
                {
                    // per-job pool
                    var jobInfo = new SparkPoolJobModel()
                    {
                        JobId = this.metadata.ComputeGovernedAssetsSparkJobBatchId,
                        PoolResourceId = this.metadata.SparkPoolId
                    };
                    jobDetails = await this.computeGovernedAssetsSparkJobComponent.GetJob(
                        jobInfo, new CancellationToken());
                }

                if (SparkJobUtils.IsSuccess(jobDetails))
                {
                    //Not enabled yet
                    this.metadata.ComputeGovernedAssetsSparkJobStatus = DataPlaneSparkJobStatus.Succeeded;
                    jobStageStatus = JobExecutionStatus.Completed;
                }
                else if (SparkJobUtils.IsFailure(jobDetails))
                {
                    this.metadata.ComputeGovernedAssetsSparkJobStatus = DataPlaneSparkJobStatus.Failed;
                    jobStageStatus = JobExecutionStatus.Failed;
                }
                else if (jobDetails.Id == -1)
                {
                    this.metadata.ComputeGovernedAssetsSparkJobStatus = DataPlaneSparkJobStatus.Failed;
                    jobStageStatus = JobExecutionStatus.Failed;
                }

                jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
                this.logger.LogTrace($"track compute governed assets spark job stage status: {jobStatusMessage}");
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"{this.StageName}|Failed to track ComputeGovernedAssets SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
                this.logger.LogError(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
        }
    }

    public bool IsStageComplete()
    {
        return !this.exposureControl.IsDataGovBillingEventEnabled(this.metadata.AccountServiceModel.Id, this.metadata.AccountServiceModel.SubscriptionId, this.metadata.AccountServiceModel.TenantId)
            || this.metadata.ComputeGovernedAssetsSparkJobStatus == DataPlaneSparkJobStatus.Succeeded
            || this.metadata.ComputeGovernedAssetsSparkJobStatus == DataPlaneSparkJobStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return int.TryParse(this.metadata.ComputeGovernedAssetsSparkJobBatchId, out int _);
    }
}
