// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

internal class TrackDataQualitySparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<SparkJobMetadata> jobCallbackUtils;

    private readonly SparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger logger;

    private readonly IDataQualitySparkJobComponent dataQualitySparkJobComponent;

    private readonly IJobManager backgroundJobManager;

    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackDataQualitySparkJobStage(
        IServiceScope scope,
        SparkJobMetadata metadata,
        JobCallbackUtils<SparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.dataQualitySparkJobComponent = scope.ServiceProvider.GetService<IDataQualitySparkJobComponent>();
        this.backgroundJobManager = scope.ServiceProvider.GetService<IJobManager>();
    }

    public string StageName => nameof(TrackDataQualitySparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;

        try
        {
            SparkBatchJob jobDetails;

            if (string.IsNullOrEmpty(this.metadata.SparkPoolId))
            {
                // per-account pre-provisioned pool
                jobDetails = await this.dataQualitySparkJobComponent.GetJob(
                    this.metadata.AccountServiceModel,
                    int.Parse(this.metadata.SparkJobBatchId),
                    new CancellationToken());
            }
            else
            {
                // per-job pool
                var jobInfo = new SparkPoolJobModel()
                {
                    JobId = this.metadata.SparkJobBatchId,
                    PoolResourceId = this.metadata.SparkPoolId
                };
                jobDetails = await this.dataQualitySparkJobComponent.GetJob(
                    jobInfo, new CancellationToken());
            }

            jobStageStatus = SparkJobUtils.DetermineJobStageStatus(jobDetails);
            jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
            this.logger.LogTrace(jobStatusMessage);
            this.metadata.IsCompleted = SparkJobUtils.IsJobCompleted(jobDetails);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"{this.StageName} | Failed to track Data Quality SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
            this.logger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
    }

    public bool IsStageComplete()
    {
        return this.metadata.IsCompleted;
    }

    public bool IsStagePreconditionMet()
    {
        return int.TryParse(this.metadata.SparkJobBatchId, out int _);
    }
}
