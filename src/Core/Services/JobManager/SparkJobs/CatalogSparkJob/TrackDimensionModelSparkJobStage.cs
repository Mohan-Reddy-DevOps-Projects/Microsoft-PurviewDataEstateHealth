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

internal class TrackDimensionModelSparkJobStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils;
    private readonly DataPlaneSparkJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IDimensionModelSparkJobComponent dimensionModelSparkJobComponent;
    private readonly IJobManager backgroundJobManager;
    private readonly IDatabaseManagementService databaseManagementService;
    private static readonly JsonSerializerOptions jsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TrackDimensionModelSparkJobStage(
    IServiceScope scope,
    DataPlaneSparkJobMetadata metadata,
    JobCallbackUtils<DataPlaneSparkJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.logger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        this.dimensionModelSparkJobComponent = scope.ServiceProvider.GetService<IDimensionModelSparkJobComponent>();
        this.backgroundJobManager = scope.ServiceProvider.GetService<IJobManager>();
        this.databaseManagementService = scope.ServiceProvider.GetService<IDatabaseManagementService>();
    }

    public string StageName => nameof(TrackDimensionModelSparkJobStage);

    public async Task<JobExecutionResult> Execute()
    {
        JobExecutionStatus jobStageStatus = JobExecutionStatus.Postponed;
        string jobStatusMessage;
        using (this.logger.LogElapsed($"Start to track dimension spark job"))
        {
            try
            {
                SparkBatchJob jobDetails = await this.dimensionModelSparkJobComponent.GetJob(
                    this.metadata.AccountServiceModel,
                    int.Parse(this.metadata.DimensionSparkJobBatchId),
                    new CancellationToken());

                if (SparkJobUtils.IsSuccess(jobDetails))
                {
                    //Not enabled yet
                    //await this.ProvisionFabricRefreshJob(this.metadata, this.metadata.AccountServiceModel);
                    this.metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Succeeded;
                    jobStageStatus = JobExecutionStatus.Completed;
                    await this.ProvisionResetDataPlaneScheduleJob(this.metadata.AccountServiceModel).ConfigureAwait(false);

                    // update SQL external table
                    await this.databaseManagementService.Initialize(this.metadata.AccountServiceModel, CancellationToken.None).ConfigureAwait(false);
                    await this.databaseManagementService.RunSetupSQL(CancellationToken.None).ConfigureAwait(false);

                    // trigger PBI refresh job
                    await this.TriggerPBIRefreshJob(this.metadata.AccountServiceModel).ConfigureAwait(false);
                }
                else if (SparkJobUtils.IsFailure(jobDetails))
                {
                    this.metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Failed;
                    jobStageStatus = JobExecutionStatus.Failed;
                }

                jobStatusMessage = SparkJobUtils.GenerateStatusMessage(this.metadata.AccountServiceModel.Id, jobDetails, jobStageStatus, this.StageName);
                this.logger.LogTrace($"track dimension spark job stage status: {jobStatusMessage}");
            }
            catch (Exception exception)
            {
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"{this.StageName}|Failed to track Dimension Model SPARK job for account: {this.metadata.AccountServiceModel.Id} in {this.StageName} with error: {exception.Message}";
                this.logger.LogError(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)));
        }


    }

    private async Task ProvisionResetDataPlaneScheduleJob(AccountServiceModel account)
    {
        await this.backgroundJobManager.ProvisionBackgroundJobResetJob(account);
    }

    private async Task TriggerPBIRefreshJob(AccountServiceModel account)
    {
        await this.backgroundJobManager.RunPBIRefreshJob(account);
    }

    private async Task ProvisionFabricRefreshJob(StagedWorkerJobMetadata metadata, AccountServiceModel account)
    {
        await this.backgroundJobManager.StartFabricelRefreshJob(metadata, account);
    }

    public bool IsStageComplete()
    {
        return this.metadata.DimensionSparkJobStatus == DataPlaneSparkJobStatus.Succeeded || this.metadata.DimensionSparkJobStatus == DataPlaneSparkJobStatus.Failed;
    }

    public bool IsStagePreconditionMet()
    {
        return int.TryParse(this.metadata.DimensionSparkJobBatchId, out int _);
    }
}
