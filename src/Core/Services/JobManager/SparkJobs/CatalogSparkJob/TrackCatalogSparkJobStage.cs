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

internal class TrackCatalogSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;
    private readonly DataPlaneSparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly ICatalogSparkJobComponent catalogSparkJobComponent;
    private readonly IJobManager backgroundJobManager;
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackCatalogSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.catalogSparkJobComponent = scope.ServiceProvider.GetService<ICatalogSparkJobComponent>();
        this.backgroundJobManager = scope.ServiceProvider.GetService<IJobManager>();
    }

    public string StageName => nameof(TrackCatalogSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus = JobExecutionStatus.Postponed;
        string jobStatusMessage;
        using (this.logger.LogElapsed($"Track catalog spark job"))
        {
            try
            {
                SparkBatchJob jobDetails;

                if (string.IsNullOrEmpty(this.metadata.SparkPoolId))
                {
                    // per-account pre-provisioned pool
                    jobDetails = await this.catalogSparkJobComponent.GetJob(
                        this.metadata.AccountServiceModel,
                        int.Parse(this.metadata.CatalogSparkJobBatchId),
                        new CancellationToken());
                }
                else
                {
                    // per-job pool
                    var jobInfo = new SparkPoolJobModel()
                    {
                        JobId = this.metadata.CatalogSparkJobBatchId,
                        PoolResourceId = this.metadata.SparkPoolId
                    };
                    jobDetails = await this.catalogSparkJobComponent.GetJob(
                        jobInfo, new CancellationToken());
                }

                this.logger.LogTrace($"Spark job status: state, {jobDetails.State}, result, {jobDetails.Result}");
                if (SparkJobUtils.IsSuccess(jobDetails))
                {
                    this.metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Succeeded;
                    jobStageStatus = JobExecutionStatus.Completed;
                }
                else if (SparkJobUtils.IsFailure(jobDetails))
                {
                    this.metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Failed;
                    jobStageStatus = JobExecutionStatus.Failed;
                }

                jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
                this.logger.LogTrace($"Track catalog spark job stage status: {jobStatusMessage}");
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"{this.StageName}|Failed to track Catalog SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
                this.logger.LogError(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
        }
    }

    public bool IsStageComplete()
    {
        return this.metadata.CatalogSparkJobStatus == DataPlaneSparkJobStatus.Succeeded || this.metadata.CatalogSparkJobStatus == DataPlaneSparkJobStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return int.TryParse(this.metadata.CatalogSparkJobBatchId, out int _);
    }
}
