﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Analytics.Synapse.Spark.Models;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.Spark;
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
    private readonly IAccountExposureControlConfigProvider exposureControl;

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
        this.exposureControl = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
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
                SparkBatchJob jobDetails;

                if (string.IsNullOrEmpty(this.metadata.SparkPoolId))
                {
                    // per-account pre-provisioned pool
                    jobDetails = await this.dimensionModelSparkJobComponent.GetJob(
                        this.metadata.AccountServiceModel,
                        int.Parse(this.metadata.DimensionSparkJobBatchId),
                        new CancellationToken());
                }
                else
                {
                    // per-job pool
                    var jobInfo = new SparkPoolJobModel()
                    {
                        JobId = this.metadata.DimensionSparkJobBatchId,
                        PoolResourceId = this.metadata.SparkPoolId
                    };
                    jobDetails = await this.dimensionModelSparkJobComponent.GetJob(
                        jobInfo, new CancellationToken());
                }

                if (SparkJobUtils.IsSuccess(jobDetails))
                {
                    this.metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Succeeded;
                    jobStageStatus = JobExecutionStatus.Completed;
                    await this.ProvisionResetDataPlaneScheduleJob(this.metadata.AccountServiceModel).ConfigureAwait(false);

                    //Removing the EC update in all cases
                    //if (this.exposureControl.IsDataGovHealthRunSetupSQLEnabled(this.metadata.AccountServiceModel.Id, this.metadata.AccountServiceModel.SubscriptionId, this.metadata.AccountServiceModel.TenantId))
                    {
                        // update SQL external table
                        await this.databaseManagementService.Initialize(this.metadata.AccountServiceModel, CancellationToken.None).ConfigureAwait(false);
                        await this.databaseManagementService.RunSetupSQL(CancellationToken.None).ConfigureAwait(false);
                    }

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
