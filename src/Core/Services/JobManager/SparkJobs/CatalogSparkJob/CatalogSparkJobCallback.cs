// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;

[JobCallback(Name = nameof(CatalogSparkJobCallback))]
internal class CatalogSparkJobCallback(IServiceScope scope) : StagedWorkerJobCallback<DataPlaneSparkJobMetadata>(scope)
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
    private readonly ISparkJobManager sparkJobManager = scope.ServiceProvider.GetService<ISparkJobManager>();
    private readonly DHAnalyticsScheduleRepository dHAnalyticsScheduleRepository = scope.ServiceProvider.GetService<DHAnalyticsScheduleRepository>();
    private readonly IServiceScope serviceScope = scope;
    private readonly IAccountExposureControlConfigProvider exposureControlConfigProvider = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();

    protected override string JobName => nameof(CatalogSparkJobCallback);

    protected override bool IsRecurringJob => true;

    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        await Task.CompletedTask;
    }
    protected override bool IsJobReachMaxExecutionTime()
    {
        if (this.Metadata.CurrentScheduleStartTime != null)
        {
            //Increasing the delete spark scenario to handle quota issue.
            return DateTime.UtcNow > this.Metadata.CurrentScheduleStartTime?.AddHours(6);
        }
        return false;
    }

    protected override async Task<bool> IsJobPreconditionMet()
    {
        return await Task.FromResult(true);
    }

    private async Task<bool> HasAnalyticsScheduleConfigured()
    {
        // If repository is not available, assume no schedule exists
        if (this.dHAnalyticsScheduleRepository == null)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"DHAnalyticsScheduleRepository is not available, assuming no analytics schedule exists");
            return false;
        }

        try
        {
            string accountId = this.Metadata.AccountServiceModel.Id;
            string tenantId = this.Metadata.AccountServiceModel.TenantId;

            this.dataEstateHealthRequestLogger.LogInformation($"Checking analytics schedule for Account ID: {accountId}, Tenant ID: {tenantId}");
            var schedules = await this.dHAnalyticsScheduleRepository.QueryAnalyticsScheduleAsync(tenantId, accountId, DHControlScheduleType.ControlGlobal);
            var hasSchedule = schedules.Any();
            this.dataEstateHealthRequestLogger.LogInformation($"Analytics schedule check completed. Has schedule: {hasSchedule}");
            return hasSchedule;
        }
        catch (Exception ex)
        {
            this.dataEstateHealthRequestLogger.LogError($"Failed to check for analytics schedule, assuming no schedule exists", ex);
            return false;
        }
    }

    protected override void OnJobConfigure()
    {
        if (exposureControlConfigProvider.IsDehEnableNewControlsFlowEnabled(this.Metadata.AccountServiceModel.Id, null, this.Metadata.AccountServiceModel.TenantId))
        {
            this.JobStages = [];
            this.dataEstateHealthRequestLogger.LogInformation($"New controls flow is enabled for account: {this.Metadata.AccountServiceModel.Id}, " +
                                                              $"tenant: {this.Metadata.AccountServiceModel.TenantId}. No job stages configured.");
            return;
        }
        this.dataEstateHealthRequestLogger.LogInformation($"New controls flow is not enabled for account: {this.Metadata.AccountServiceModel.Id}, " +
                                                              $"tenant: {this.Metadata.AccountServiceModel.TenantId}. Configuring job stages.");
        this.dataEstateHealthRequestLogger.LogInformation($"Configuring job stages for account: {this.Metadata.AccountServiceModel.Id}, tenant: {this.Metadata.AccountServiceModel.TenantId}");
        
        JobSubmissionEvaluator jobSubmissionEvaluator = new JobSubmissionEvaluator(this.serviceScope);
        var isStorageSyncConfigured = jobSubmissionEvaluator.IsStorageSyncEnabledAndConfigured(this.Metadata.AccountServiceModel.Id,
            this.Metadata.AccountServiceModel.TenantId).GetAwaiter().GetResult();
        var isDEHRanInLast24Hours = jobSubmissionEvaluator.IsDEHRanInLast24Hours(this.Metadata.AccountServiceModel.Id).GetAwaiter().GetResult();
        var hasAnalyticsSchedule = this.HasAnalyticsScheduleConfigured().GetAwaiter().GetResult();
        
        this.dataEstateHealthRequestLogger.LogInformation($"Job configuration preconditions: isStorageSyncConfigured={isStorageSyncConfigured}, isDEHRanInLast24Hours={isDEHRanInLast24Hours}, hasAnalyticsSchedule={hasAnalyticsSchedule}");
        
        // For CatalogSparkJobCallback, we use IsStorageSyncEnabledAndConfigured which only returns true if configured AND not disabled
        if (isStorageSyncConfigured)
        {
            // Base stages that are always included
            var stages = new List<IJobCallbackStage>
            {
                new TriggerCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TrackCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TriggerDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TrackDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            };
            
            // Only add Fabric stages if no analytics schedule is configured
            if (!hasAnalyticsSchedule)
            {
                this.dataEstateHealthRequestLogger.LogInformation("No analytics schedule configured, adding Fabric Spark job stages");
                stages.Add(new TriggerFabricSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
                stages.Add(new TrackFabricSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils));
            }
            else
            {
                this.dataEstateHealthRequestLogger.LogInformation("Analytics schedule configured, skipping Fabric Spark job stages");
            }
            
            this.JobStages = stages.ToArray();
            this.dataEstateHealthRequestLogger.LogInformation($"Configured job with {stages.Count} stages for isStorageSyncConfigured=true path");
        }
        else if (isDEHRanInLast24Hours)
        {
            // If storage sync is disabled or not configured, but DEH ran recently, we still process catalog/dimension stages
            this.JobStages = [
                new TriggerCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TrackCatalogSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TriggerDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
                new TrackDimensionModelSparkJobStage(this.Scope, this.Metadata, this.JobCallbackUtils),
            ];
            this.dataEstateHealthRequestLogger.LogInformation($"Configured job with 4 stages for isDEHRanInLast24Hours=true path");
        }
        else
        {
            this.JobStages = [];
            this.dataEstateHealthRequestLogger.LogInformation("No job stages configured as no preconditions were met");
        }
        
        this.dataEstateHealthRequestLogger.LogInformation($"Job stage configuration completed with {this.JobStages.Count()} total stages");
    }

    protected override async Task TransitionToJobFailed()
    {
        using (this.DataEstateHealthRequestLogger.LogElapsed($"Transition to job failed, name: {this.JobName}"))
        {
            await this.DeleteSparkPools();
            this.ResetJobWorkingState();
        }
    }

    protected override async Task TransitionToJobSucceeded()
    {
        using (this.DataEstateHealthRequestLogger.LogElapsed($"Transition to job succeeded, name: {this.JobName}"))
        {
            await this.DeleteSparkPools();
            this.ResetJobWorkingState();
        }
    }

    private void ResetJobWorkingState()
    {
        this.Metadata.CatalogSparkJobBatchId = string.Empty;
        this.Metadata.DimensionSparkJobBatchId = string.Empty;
        this.Metadata.FabricSparkJobBatchId = string.Empty;
        this.Metadata.SparkPoolId = string.Empty;
        this.Metadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.FabricSparkJobStatus = DataPlaneSparkJobStatus.Others;
        this.Metadata.CurrentScheduleStartTime = null;
    }

    private async Task DeleteSparkPools()
    {
        // Delete spark pools
        try
        {
            await this.sparkJobManager.DeleteSparkPool(this.Metadata.SparkPoolId, new CancellationToken());
        }
        catch (Exception e)
        {
            this.dataEstateHealthRequestLogger.LogError($"Failed to delete spark pool. SparkPoolId: {this.Metadata.SparkPoolId}", e);
        }
    }
}
