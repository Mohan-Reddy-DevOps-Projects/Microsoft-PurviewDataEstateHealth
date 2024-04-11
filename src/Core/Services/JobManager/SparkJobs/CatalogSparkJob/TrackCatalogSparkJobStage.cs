// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

internal class TrackCatalogSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<SparkJobMetadata> jobCallbackUtils;
    private readonly SparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly ICatalogSparkJobComponent catalogSparkJobComponent;
    private readonly IJobManager backgroundJobManager;
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackCatalogSparkJobStage(
    IServiceScope scope,
    SparkJobMetadata metadata,
    JobCallbackUtils<SparkJobMetadata> jobCallbackUtils)
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
        JobExecutionStatus jobStageStatus;
        string jobStatusMessage;

        try
        {
            SparkBatchJob jobDetails = await this.catalogSparkJobComponent.GetJob(
                this.metadata.AccountServiceModel,
                int.Parse(this.metadata.SparkJobBatchId),
                new CancellationToken());

            jobStageStatus = SparkJobUtils.DetermineJobStageStatus(jobDetails);
            jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
            this.logger.LogTrace(jobStatusMessage);

            if (SparkJobUtils.IsSuccess(jobDetails))
            {
                await this.ProvisionDimensionModelRefreshJob(this.metadata, this.metadata.AccountServiceModel);
            }

            this.metadata.IsCompleted = SparkJobUtils.IsJobCompleted(jobDetails);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"{this.StageName}|Failed to track Catalog SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
            this.logger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
    }

    private async Task ProvisionDimensionModelRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel account)
    {
        await this.backgroundJobManager.StartDimensionModelRefreshJob(metadata, account);
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
