// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Services.JobManager.SparkJobs.CatalogSparkJob;
using System;
using System.Threading.Tasks;
using WindowsAzure.ResourceStack.Common.Json;

[JobCallback(Name = nameof(DehScheduleCallback))]
internal class DehScheduleCallback(IServiceScope scope) : StagedWorkerJobCallback<DehScheduleJobMetadata>(scope)
{
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();

    /// <inheritdoc />
    protected override string JobName => nameof(DehScheduleCallback);
    private readonly IAccountExposureControlConfigProvider accountExposureControlConfigProvider = scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
    private readonly IJobManager jobManager = scope.ServiceProvider.GetService<IJobManager>();
    private readonly DHAnalyticsScheduleRepository dHAnalyticsScheduleRepository = scope.ServiceProvider.GetService<DHAnalyticsScheduleRepository>();
    private const string CatalogSparkJobs = "-CATALOG-SPARK-JOBS";
    private const string SparkJob = "-CATALOG-SPARK-JOB";

    /// <inheritdoc />
    protected override async Task FinalizeJob(JobExecutionResult result, Exception exception)
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] FinalizeJob called with result: {result?.Status}, exception: {exception?.Message}");
        await Task.CompletedTask;
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] FinalizeJob completed");
    }

    /// <inheritdoc />
    protected override bool IsJobReachMaxExecutionTime()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] IsJobReachMaxExecutionTime called, returning false");
        return false;
    }

    /// <inheritdoc />
    protected override async Task<bool> IsJobPreconditionMet()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] IsJobPreconditionMet called, returning true");
        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    protected override void OnJobConfigure()
    {

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] OnJobConfigure started");

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Metadata - ScheduleAccountId: {this.Metadata.ScheduleAccountId}, ScheduleTenantId: {this.Metadata.ScheduleTenantId}");
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Checking if DEH enable new controls flow is enabled...");
        bool isDehNewControlsFlowEnabled = this.accountExposureControlConfigProvider.IsDehEnableNewControlsFlowEnabled(
            this.Metadata.ScheduleAccountId, null, this.Metadata.ScheduleTenantId);
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] IsDehEnableNewControlsFlowEnabled result: {isDehNewControlsFlowEnabled}");

        if (isDehNewControlsFlowEnabled)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Taking NEW CONTROLS FLOW path");
            
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Getting DataPlaneSparkJobMetadata...");
            var catalogSparkJobMetadata = this.GetDataPlaneSparkJobMetadata();
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] DataPlaneSparkJobMetadata retrieved: {(catalogSparkJobMetadata != null ? "SUCCESS" : "NULL")}");

            SetParentRootTraceIdToChildren(catalogSparkJobMetadata, this.Metadata);
            
            if (catalogSparkJobMetadata != null)
            {
                this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] CatalogSparkJobMetadata details - AccountId: {catalogSparkJobMetadata.AccountServiceModel?.Id}, TenantId: {catalogSparkJobMetadata.AccountServiceModel?.TenantId}");
            }
            
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Creating job utils...");
            var catalogSparkJobUtils = new JobCallbackUtils<DataPlaneSparkJobMetadata>(catalogSparkJobMetadata);
            var dehScheduleJobUtils = this.GetDehScheduleJobUtils();
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Job utils created successfully");
            
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Queuing Full DEH Data Refresh Pipeline...");
            this.QueueFullDehDataRefreshPipeline(catalogSparkJobMetadata, catalogSparkJobUtils, dehScheduleJobUtils);
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Full DEH Data Refresh Pipeline queued successfully");
        }
        else
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Taking LEGACY FLOW path");
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Creating single DehScheduleStage...");
            
            this.JobStages = new List<IJobCallbackStage>
            {
                new DehScheduleStage(this.Scope, this.Metadata, this.JobCallbackUtils, this.CancellationToken)
            };
            
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Legacy flow configured with 1 stage (DehScheduleStage)");
        }
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] OnJobConfigure completed with {this.JobStages?.Count() ?? 0} total stages");
    }

    private JobCallbackUtils<DehScheduleJobMetadata> GetDehScheduleJobUtils()
    {
        var dehScheduleJobUtils = new JobCallbackUtils<DehScheduleJobMetadata>(this.Metadata);
        return dehScheduleJobUtils;
    }

    private DataPlaneSparkJobMetadata GetDataPlaneSparkJobMetadata()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] GetDataPlaneSparkJobMetadata started");
        
        var catalogSparkJobMetadata = this.Metadata.CatalogSparkJobMetadata;
        if (catalogSparkJobMetadata != null)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] CatalogSparkJobMetadata found in Metadata, using existing metadata");
            return catalogSparkJobMetadata;
        }

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] CatalogSparkJobMetadata not found in Metadata, attempting to retrieve from JobManager...");
        
        string catalogSparkJobsKey = this.Metadata.RequestContext.AccountId + CatalogSparkJobs;
        string sparkJobKey = this.Metadata.RequestContext.AccountId + SparkJob;
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Retrieving job with keys - CatalogSparkJobsKey: '{catalogSparkJobsKey}', SparkJobKey: '{sparkJobKey}'");
        
        var catalogSparkJob = this.jobManager.GetJobAsync(catalogSparkJobsKey, sparkJobKey).GetAwaiter().GetResult();
        
        if (catalogSparkJob == null)
        {
            this.dataEstateHealthRequestLogger.LogWarning($"[{this.JobName}] CatalogSparkJob not found in JobManager, returning null metadata");
            return catalogSparkJobMetadata;
        }

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] CatalogSparkJob found, extracting metadata...");
        catalogSparkJobMetadata = catalogSparkJob.GetMetadata<DataPlaneSparkJobMetadata>();
        this.Metadata.CatalogSparkJobMetadata = catalogSparkJobMetadata;

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] CatalogSparkJobMetadata extracted and cached successfully");
        return catalogSparkJobMetadata;
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobFailed()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] TransitionToJobFailed started");
        await Task.CompletedTask;
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} failed.");
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Resetting CatalogSparkJobMetadata state...");
        this.ResetMetadataState();
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] TransitionToJobFailed completed");
    }

    /// <inheritdoc />
    protected override async Task TransitionToJobSucceeded()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] TransitionToJobSucceeded started");
        await Task.CompletedTask;
        this.dataEstateHealthRequestLogger.LogInformation($"{this.JobName} succeeded.");
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Resetting CatalogSparkJobMetadata state...");
        this.ResetMetadataState();
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] TransitionToJobSucceeded completed");
    }

    private void ResetMetadataState()
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] ResetMetadataState started");
        
        if (this.Metadata.CatalogSparkJobMetadata is not { } catalogMetadata)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] No CatalogSparkJobMetadata to reset");
        }
        else
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Resetting all Metadata fields to default values...");

            catalogMetadata.CatalogSparkJobBatchId = String.Empty;
            catalogMetadata.DimensionSparkJobBatchId = String.Empty;
            catalogMetadata.FabricSparkJobBatchId = String.Empty;
            catalogMetadata.SparkPoolId = String.Empty;
            catalogMetadata.CatalogSparkJobStatus = DataPlaneSparkJobStatus.Others;
            catalogMetadata.DimensionSparkJobStatus = DataPlaneSparkJobStatus.Others;
            catalogMetadata.FabricSparkJobStatus = DataPlaneSparkJobStatus.Others;
            catalogMetadata.CurrentScheduleStartTime = null;
            catalogMetadata.RootTraceId = String.Empty;
        }

        // Reset the separate workflow metadata
        this.Metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobBatchId = String.Empty;
        this.Metadata.ControlsWorkflowMetadata.DqRulesEvaluationSparkJobStatus = ControlsWorkflowStatus.NotStarted;
        this.Metadata.ActionsGenerationMetadata.ActionsGenerationWorkflowStatus = ActionsGenerationWorkflowStatus.NotStarted;
        this.Metadata.SqlGenerationMetadata.SqlGenerationStatus = SqlGenerationWorkflowStatus.NotStarted;
        this.Metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId = String.Empty;

        //reset the root trace id
        this.Metadata.RootTraceId = String.Empty;

        this.dataEstateHealthRequestLogger.LogInformation("Reset CatalogSparkJobMetadata state");
    }

    private void QueueFullDehDataRefreshPipeline(DataPlaneSparkJobMetadata dataPlaneSparkJobMetadata, JobCallbackUtils<DataPlaneSparkJobMetadata> catalogSparkJobUtils, JobCallbackUtils<DehScheduleJobMetadata> dehScheduleJobUtils)
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] QueueFullDehDataRefreshPipeline started");
        
        if (dataPlaneSparkJobMetadata?.AccountServiceModel == null)
        {
            this.dataEstateHealthRequestLogger.LogError($"[{this.JobName}] DataPlaneSparkJobMetadata or AccountServiceModel is null, cannot configure pipeline");
            this.JobStages = [];
            return;
        }
        
        this.dataEstateHealthRequestLogger.LogInformation($"Configuring job stages for account: {dataPlaneSparkJobMetadata.AccountServiceModel.Id}, " +
                                                          $"tenant: {dataPlaneSparkJobMetadata.AccountServiceModel.TenantId}");

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Creating JobSubmissionEvaluator...");
        var jobSubmissionEvaluator = new JobSubmissionEvaluator(this.Scope);
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Evaluating preconditions...");
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Checking if storage sync is configured...");
        bool isStorageSyncConfigured = IsStorageSyncConfigured(dataPlaneSparkJobMetadata, jobSubmissionEvaluator);
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Storage sync configured check result: {isStorageSyncConfigured}");
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Checking if DEH ran in last 24 hours...");
        bool isDehRanInLast24Hours = jobSubmissionEvaluator.IsDEHRanInLast24Hours(dataPlaneSparkJobMetadata.AccountServiceModel.Id).GetAwaiter().GetResult();
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] DEH ran in last 24 hours check result: {isDehRanInLast24Hours}");
        
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Checking if analytics schedule is configured...");
        bool hasAnalyticsSchedule = this.HasAnalyticsScheduleConfigured(dataPlaneSparkJobMetadata).GetAwaiter().GetResult();
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Analytics schedule configured check result: {hasAnalyticsSchedule}");

        this.dataEstateHealthRequestLogger.LogInformation($"Job configuration preconditions: " +
                                                          $"isStorageSyncConfigured={isStorageSyncConfigured}, " +
                                                          $"isDEHRanInLast24Hours={isDehRanInLast24Hours}, hasAnalyticsSchedule={hasAnalyticsSchedule}");

        // For CatalogSparkJobCallback, we use IsStorageSyncEnabledAndConfigured which only returns true if configured AND not disabled
        if (isStorageSyncConfigured)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Taking STORAGE SYNC CONFIGURED path");

            // Base stages that are always included
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Creating base stages for storage sync configured path...");
            var stages = new List<IJobCallbackStage>
            {
                new TriggerCatalogSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
                new TrackCatalogSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
                new TriggerMdqRulesSpecificationStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TriggerControlsDqRulesEvaluationSparkJobStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TrackTriggeredControlsDqRulesEvaluationSparkJobStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TriggerActionsUpsertWorkflowStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TriggerDimensionModelSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
                new TrackDimensionModelSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
            };
            
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Base stages created: {stages.Count} stages");

            // Only add Fabric stages if no analytics schedule is configured
            if (!hasAnalyticsSchedule)
            {
                this.dataEstateHealthRequestLogger.LogInformation("No analytics schedule configured, adding Fabric Spark job stages");
                stages.Add(new TriggerFabricSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils));
                stages.Add(new TrackFabricSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils));
                this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Added 2 Fabric stages, total stages now: {stages.Count}");
            }
            else
            {
                this.dataEstateHealthRequestLogger.LogInformation("Analytics schedule configured, skipping Fabric Spark job stages");
            }

            this.JobStages = stages.ToArray();
            this.dataEstateHealthRequestLogger.LogInformation($"Configured job with {stages.Count} stages for isStorageSyncConfigured=true path");
        }
        else if (isDehRanInLast24Hours)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Taking DEH RAN IN LAST 24 HOURS path");
            
            // If storage sync is disabled or not configured, but DEH ran recently, we still process catalog/dimension stages
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Creating stages for DEH ran in last 24 hours path...");
            var stages = new List<IJobCallbackStage>
            {
                new TriggerCatalogSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
                new TrackCatalogSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
                new TriggerMdqRulesSpecificationStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TriggerControlsDqRulesEvaluationSparkJobStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TrackTriggeredControlsDqRulesEvaluationSparkJobStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TriggerActionsUpsertWorkflowStage(this.Scope, this.Metadata, dehScheduleJobUtils),
                new TriggerDimensionModelSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
                new TrackDimensionModelSparkJobStage(this.Scope, dataPlaneSparkJobMetadata, catalogSparkJobUtils),
            };

            this.JobStages = stages.ToArray();
            this.dataEstateHealthRequestLogger.LogInformation($"Configured job with {stages.Count} stages for isDEHRanInLast24Hours=true path");
        }
        else
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Taking NO PRECONDITIONS MET path");
            this.JobStages = [];
            this.dataEstateHealthRequestLogger.LogInformation("No job stages configured as no preconditions were met");
        }

        this.dataEstateHealthRequestLogger.LogInformation($"Job stage configuration completed with {this.JobStages.Count()} total stages");
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] QueueFullDehDataRefreshPipeline completed");
    }

    private static bool IsStorageSyncConfigured(DataPlaneSparkJobMetadata dataPlaneSparkJobMetadata, JobSubmissionEvaluator jobSubmissionEvaluator)
    {
        bool isStorageSyncConfigured = jobSubmissionEvaluator.IsStorageSyncEnabledAndConfigured(dataPlaneSparkJobMetadata.AccountServiceModel.Id,
            dataPlaneSparkJobMetadata.AccountServiceModel.TenantId).GetAwaiter().GetResult();
        return isStorageSyncConfigured;
    }

    private async Task<bool> HasAnalyticsScheduleConfigured(DataPlaneSparkJobMetadata dataPlaneSparkJobMetadata)
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] HasAnalyticsScheduleConfigured started");
        
        // If repository is not available, assume no schedule exists
        if (this.dHAnalyticsScheduleRepository == null)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"DHAnalyticsScheduleRepository is not available, assuming no analytics schedule exists");
            return false;
        }

        try
        {
            string accountId = dataPlaneSparkJobMetadata.AccountServiceModel.Id;
            string tenantId = dataPlaneSparkJobMetadata.AccountServiceModel.TenantId;

            this.dataEstateHealthRequestLogger.LogInformation($"Checking analytics schedule for Account ID: {accountId}, Tenant ID: {tenantId}");
            
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Querying analytics schedule with ScheduleType: {DHControlScheduleType.ControlGlobal}");
            var schedules = await this.dHAnalyticsScheduleRepository.QueryAnalyticsScheduleAsync(tenantId, accountId, DHControlScheduleType.ControlGlobal);

            var dhControlScheduleStoragePayloadWrappers = schedules.ToList();
            bool hasSchedule = dhControlScheduleStoragePayloadWrappers.Count != 0;
            this.dataEstateHealthRequestLogger.LogInformation($"Analytics schedule check completed. Has schedule: {hasSchedule}");
            
            if (hasSchedule)
            {
                this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Found {dhControlScheduleStoragePayloadWrappers.Count} analytics schedules");
            }
            
            return hasSchedule;
        }
        catch (Exception ex)
        {
            this.dataEstateHealthRequestLogger.LogError($"Failed to check for analytics schedule, assuming no schedule exists", ex);
            return false;
        }
    }

    protected override async Task<JobExecutionResult> OnJobExecutionResult(JobExecutionResult result, Exception exception)
    {
        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] OnJobExecutionResult called with Status: {result?.Status}, Exception: {exception?.Message}");

        var baseDecoratedResult = await base.OnJobExecutionResult(result, exception);

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Base decorated result Status: {baseDecoratedResult?.Status}");

        if (baseDecoratedResult != null && baseDecoratedResult.Status != JobExecutionStatus.Postponed)
        {
            this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] Job not postponed, returning base result");
            return baseDecoratedResult;
        }

        this.DataEstateHealthRequestLogger.LogInformation($"Setting NextMetadata for postponed job: {this.JobName}");
        if (baseDecoratedResult == null)
        {
            throw new InvalidOperationException($"[{this.JobName}] OnJobExecutionResult called with null baseDecoratedResult, cannot proceed");
        }

        baseDecoratedResult.NextMetadata = this.Metadata.ToJson();

        this.dataEstateHealthRequestLogger.LogInformation($"[{this.JobName}] OnJobExecutionResult completed for postponed job");
        return baseDecoratedResult;
    }

    private static void SetParentRootTraceIdToChildren(StagedWorkerJobMetadata parent, StagedWorkerJobMetadata child)
    {
        if (String.IsNullOrEmpty(parent.RootTraceId))
        {
            parent.RootTraceId = Guid.NewGuid().ToString();
        }
        child.RootTraceId = parent.RootTraceId;
    }
}
