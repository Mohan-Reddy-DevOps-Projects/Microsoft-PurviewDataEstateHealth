// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using global::Azure.Analytics.Synapse.Spark.Models;
using System.Text.Json;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

internal class TrackCatalogSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<CatalogSparkJobMetadata> jobCallbackUtils;

    private readonly CatalogSparkJobMetadata metadata;

    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;

    private readonly ICatalogSparkJobComponent catalogSparkJobComponent;

    private readonly IJobManager backgroundJobManager;

    public TrackCatalogSparkJobStage(
    IServiceScope scope,
    CatalogSparkJobMetadata metadata,
    JobCallbackUtils<CatalogSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
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
            this.metadata.SparkJobResult = jobDetails?.Result;

            if (jobDetails?.Result == SparkBatchJobResultType.Succeeded)
            {
                jobStageStatus = JobExecutionStatus.Succeeded;
                jobStatusMessage = $"Catalog SPARK job succeeded for account: {this.metadata.AccountServiceModel.Id} in {this.StageName}.";
                this.dataEstateHealthRequestLogger.LogTrace(jobStatusMessage);

                await this.ProvisionPBIRefreshJob(this.metadata.AccountServiceModel);
            }
            else if (jobDetails?.Result == SparkBatchJobResultType.Failed || jobDetails?.State == LivyStates.Dead)
            {
                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"Catalog SPARK job failed for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with details: {JsonSerializer.Serialize(jobDetails)}";
                this.dataEstateHealthRequestLogger.LogError(jobStatusMessage);
            }
            else
            {
                jobStageStatus = JobExecutionStatus.Postponed;
                jobStatusMessage = $"Catalog SPARK job for account: {this.metadata.AccountServiceModel.Id} is still in progress with status: {this.metadata.SparkJobResult}";
            }
        }
        catch (Exception exception)
        {
            jobStageStatus = JobExecutionStatus.Completed;
            jobStatusMessage = $"Failed to submit Catalog SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
            this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
        }

        return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
    }

    private async Task ProvisionPBIRefreshJob(AccountServiceModel account)
    {
        await backgroundJobManager.StartPBIRefreshJob(account);
    }

    public bool IsStageComplete()
    {
        return this.metadata.SparkJobResult == SparkBatchJobResultType.Succeeded || this.metadata.SparkJobResult == SparkBatchJobResultType.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return int.TryParse(this.metadata.SparkJobBatchId, out int _);
    }
}
