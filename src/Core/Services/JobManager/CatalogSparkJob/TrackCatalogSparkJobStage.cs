// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

internal class TrackCatalogSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<CatalogSparkJobMetadata> jobCallbackUtils;
    private readonly CatalogSparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly ICatalogSparkJobComponent catalogSparkJobComponent;
    private readonly IJobManager backgroundJobManager;
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackCatalogSparkJobStage(
    IServiceScope scope,
    CatalogSparkJobMetadata metadata,
    JobCallbackUtils<CatalogSparkJobMetadata> jobCallbackUtils)
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

            jobStageStatus = DetermineJobStageStatus(jobDetails);
            jobStatusMessage = this.GenerateStatusMessage(jobDetails, jobStageStatus);
            this.logger.LogTrace(jobStatusMessage);

            if (IsSuccess(jobDetails))
            {
                await this.ProvisionPBIRefreshJob(this.metadata, this.metadata.AccountServiceModel);
            }

            this.metadata.IsCompleted = IsJobCompleted(jobDetails);
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"{this.StageName}|Failed to track Catalog SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
            this.logger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
    }

    private async Task ProvisionPBIRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel account)
    {
        await this.backgroundJobManager.StartPBIRefreshJob(metadata, account);
    }

    public bool IsStageComplete()
    {
        return this.metadata.IsCompleted;
    }

    public bool IsStagePreconditionMet()
    {
        return int.TryParse(this.metadata.SparkJobBatchId, out int _);
    }

    private static JobExecutionStatus DetermineJobStageStatus(SparkBatchJob jobDetails)
    {
        if (IsSuccess(jobDetails))
        {
            return JobExecutionStatus.Succeeded;
        }
        if (IsFailure(jobDetails))
        {
            return JobExecutionStatus.Completed;
        }

        return JobExecutionStatus.Postponed;
    }

    private static bool IsSuccess(SparkBatchJob jobDetails)
    {
        return jobDetails != null && (jobDetails.Result == SparkBatchJobResultType.Succeeded || jobDetails.State == LivyStates.Success);
    }

    private static bool IsFailure(SparkBatchJob jobDetails)
    {
        return jobDetails != null && (jobDetails.Result == SparkBatchJobResultType.Failed || jobDetails.State == LivyStates.Dead || jobDetails.State == LivyStates.Killed);
    }

    private static bool IsJobCompleted(SparkBatchJob jobDetails)
    {
        return IsSuccess(jobDetails) || IsFailure(jobDetails);
    }

    private string GenerateStatusMessage(SparkBatchJob jobDetails, JobExecutionStatus status)
    {
        string accountId = this.metadata.AccountServiceModel.Id;
        string detail = status switch
        {
            JobExecutionStatus.Succeeded => $"{this.StageName}|{status} for account: {accountId}.",
            JobExecutionStatus.Completed => $"{this.StageName}|failed for account: {accountId} with details: {JsonSerializer.Serialize(jobDetails, jsonOptions)}",
            JobExecutionStatus.Postponed => $"{this.StageName}|{status} for account: {accountId}.",
            _ => $"Unknown status for {this.StageName} for account: {accountId}."
        };
        return detail;
    }
}
