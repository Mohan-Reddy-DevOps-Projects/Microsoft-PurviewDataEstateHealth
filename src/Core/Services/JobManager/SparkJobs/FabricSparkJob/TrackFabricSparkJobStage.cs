// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

internal class TrackFabricSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;
    private readonly DataPlaneSparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IFabricSparkJobComponent fabricSparkJobComponent;
    private readonly IJobManager backgroundJobManager;
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackFabricSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.fabricSparkJobComponent = scope.ServiceProvider.GetService<IFabricSparkJobComponent>();
        this.backgroundJobManager = scope.ServiceProvider.GetService<IJobManager>();
    }

    public string StageName => nameof(TrackFabricSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus = JobExecutionStatus.Postponed;
        string jobStatusMessage;

        try
        {
            SparkBatchJob jobDetails;

            if (string.IsNullOrEmpty(this.metadata.SparkPoolId))
            {
                // per-account pre-provisioned pool
                jobDetails = await this.fabricSparkJobComponent.GetJob(
                    this.metadata.AccountServiceModel,
                    int.Parse(this.metadata.FabricSparkJobBatchId),
                    new CancellationToken());
            }
            else
            {
                // per-job pool
                var jobInfo = new SparkPoolJobModel()
                {
                    JobId = this.metadata.FabricSparkJobBatchId,
                    PoolResourceId = this.metadata.SparkPoolId
                };
                jobDetails = await this.fabricSparkJobComponent.GetJob(
                    jobInfo, new CancellationToken());
            }
                   
            if (SparkJobUtils.IsSuccess(jobDetails))
            {
                this.metadata.FabricSparkJobStatus = DataPlaneSparkJobStatus.Succeeded;
                jobStageStatus = JobExecutionStatus.Completed;
                await this.ProvisionResetDataPlaneScheduleJob(this.metadata.AccountServiceModel).ConfigureAwait(false);                
            }
            else if (SparkJobUtils.IsFailure(jobDetails))
            {
                this.metadata.FabricSparkJobStatus = DataPlaneSparkJobStatus.Failed;
                jobStageStatus = JobExecutionStatus.Failed;
            }

            jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
            this.logger.LogTrace($"track dimension spark job stage status: {jobStatusMessage}");


        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"{this.StageName}|Failed to track Fabric SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
            this.logger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
    }

    public bool IsStageComplete()
    {
        //return this.metadata.IsCompleted;
        //return this.metadata.FabricSparkJobStatus == DataPlaneSparkJobStatus.Succeeded || this.metadata.FabricSparkJobStatus == DataPlaneSparkJobStatus.Failed;
        //FabricSparkJobBatchId == "-1" when no config is set
        return this.metadata.FabricSparkJobBatchId == "-1" || this.metadata.FabricSparkJobStatus == DataPlaneSparkJobStatus.Succeeded || this.metadata.FabricSparkJobStatus == DataPlaneSparkJobStatus.Failed;


    }

    private async Task ProvisionResetDataPlaneScheduleJob(AccountServiceModel account)
    {
        await this.backgroundJobManager.ProvisionBackgroundJobResetJob(account);
    }

    public bool IsStagePreconditionMet()
    {
        //return int.TryParse(this.metadata.SparkJobBatchId, out int _);
        return int.TryParse(this.metadata.FabricSparkJobBatchId, out int _);
    }
}
