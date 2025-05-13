#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.MDQJob;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DHScheduleService(
    DHScoreService scoreService,
    DHScheduleInternalService scheduleService,
    DHControlScheduleRepository dhControlScheduleRepository,
    IDataQualityExecutionService dataQualityExecutionService,
    DHMonitoringService monitoringService,
    DHControlService controlService,
    DHAssessmentService assessmentService,
    IDataEstateHealthRequestLogger logger,
    IRequestHeaderContext requestHeaderContext,
    IDataQualityScoreRepository dataQualityScoreRepository,
    DHScoreRepository dhScoreRepository,
    TriggeredScheduleQueue triggeredScheduleQueue,
    DHDataEstateHealthRepository dhDataEstateHealthRepository,
    IAccountExposureControlConfigProvider accountExposureControlConfigProvider
    )
{
    private const string MDQEventOperationName = "MDQ event";

    public async Task EnqueueScheduleAsync(DHScheduleCallbackPayload payload, string tenantId, string accountId)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(EnqueueScheduleAsync)}"))
        {
            var entity = new DHScheduleQueueEntity
            {
                TenantId = tenantId.ToString(),
                AccountId = accountId.ToString(),
                ControlId = payload.ControlId,
                Operator = payload.Operator,
                TriggerType = payload.TriggerType.ToString(),
            };
            logger.LogInformation($"Enqueue schedule entity. {JsonConvert.SerializeObject(entity)}");
            await triggeredScheduleQueue.SendMessageAsync(JsonConvert.SerializeObject(entity)).ConfigureAwait(false);
            logger.LogInformation("Global schedule is enqueued successfully.");
        }
    }

    public async Task<string> TriggerScheduleJobCallbackAsync(DHScheduleCallbackPayload payload, bool isTriggeredFromGeneva=false)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(TriggerScheduleJobCallbackAsync)}"))
        {
            try
            {
                string scheduleRunId = Guid.NewGuid().ToString();
                
                // Step 1: Check if business domain container has any documents
                if (!isTriggeredFromGeneva)
                {
                    bool hasDocuments = await this.skipIfNoBusinessDomainExists(scheduleRunId);
                    if (!hasDocuments)
                    {
                        return scheduleRunId;
                    }
                }

                // Step 2 query all controls
                var controls = new List<DHControlNodeWrapper>();
                var controlGroups = new List<DHControlGroupWrapper>();

                if (string.IsNullOrEmpty(payload.ControlId))
                {
                    var result = await controlService.ListControlsAsync().ConfigureAwait(false);
                    var controlNodes = result.Results.Where(item => item is DHControlNodeWrapper).OfType<DHControlNodeWrapper>();
                    controls.AddRange(controlNodes);
                    controlGroups.AddRange(result.Results.Where(item => item is DHControlGroupWrapper).OfType<DHControlGroupWrapper>());
                    logger.LogInformation($"Trigger batch controls jobs. Count {controls.Count}.");
                }
                else
                {
                    var result = await controlService.GetControlByIdAsync(payload.ControlId).ConfigureAwait(false);
                    controls.Add((DHControlNodeWrapper)result);
                    logger.LogInformation($"Trigger control job. ControlId {payload.ControlId}.");
                }
                
                logger.LogInformation($"New scheduleRunId generated {scheduleRunId}");

                var assessments = await assessmentService.ListAssessmentsAsync().ConfigureAwait(false);
                int failedJobsCount = 0;

                // deal with DQ control group, 34115f73-b8d3-44e1-abc9-c5df18ee3eee is DQ template ID
                var dqGroup = controlGroups.Find((group) => group.SystemTemplateEntityId == DHModelConstants.CONTROL_TEMPLATE_ID_DQGROUP && group.Status == DHControlStatus.Enabled);
                if (dqGroup != null)
                {
                    _ = this.UpdateDQGroupScoreAsync(scheduleRunId, dqGroup);
                }

                var DQControlList = new Dictionary<string, DHControlNodeWrapper>();

                // Get exposure control flags once before the loop for better performance
                var accountId = requestHeaderContext.AccountObjectId.ToString();
                var tenantId = requestHeaderContext.TenantId.ToString();
                bool businessOKRsAlignmentEnabled = accountExposureControlConfigProvider.IsDEHBusinessOKRsAlignmentEnabled(accountId, string.Empty, tenantId);
                bool criticalDataIdentificationEnabled = accountExposureControlConfigProvider.IsDEHCriticalDataIdentificationEnabled(accountId, string.Empty, tenantId);

                foreach (var control in controls)
                {
                    try
                    {
                        logger.LogInformation($"start with control, Id: {control.Id}. AssessmentId: {control.AssessmentId}");
                        
                        // Check if it's one of our special controls with EC flags enabled
                        if (string.Equals(control.Name, DHControlConstants.BusinessOKRsAlignment, StringComparison.OrdinalIgnoreCase) && 
                            businessOKRsAlignmentEnabled && control.Status == DHControlStatus.InDevelopment)
                        {
                            logger.LogInformation($"Processing {DHControlConstants.BusinessOKRsAlignment} control because EC flag is enabled, regardless of control status.");
                        }
                        else if (string.Equals(control.Name, DHControlConstants.CriticalDataIdentification, StringComparison.OrdinalIgnoreCase) && 
                                 criticalDataIdentificationEnabled && control.Status == DHControlStatus.InDevelopment)
                        {
                            logger.LogInformation($"Processing {DHControlConstants.CriticalDataIdentification} control because EC flag is enabled, regardless of control status.");
                        }
                        else if (control.Status != DHControlStatus.Enabled)
                        {
                            logger.LogInformation($"control is not enabled, skip. ControlId: {control.Id}. AssessmentId: {control.AssessmentId}");
                            continue;
                        }

                        var assessment = assessments.Results.First(item => item.Id == control.AssessmentId);
                        if (assessment.TargetQualityType == DHAssessmentQualityType.DataQuality)
                        {
                            var dimension = DQDimentionConvert.ConvertCheckPointToDQDimension((assessment.Rules.First()?.Rule as DHSimpleRuleWrapper)?.CheckPoint);
                            DQControlList.Add(dimension, control);
                            continue;
                        }

                        if (assessment.Rules == null || !assessment.Rules.Any())
                        {
                            logger.LogInformation($"Control has no rules, skip. ControlId: {control.Id}. AssessmentId: {control.AssessmentId}");
                            continue;
                        }

                        // Step 3: save into monitoring table
                        var jobId = Guid.NewGuid().ToString();
                        var jobWrapper = new DHComputingJobWrapper();
                        jobWrapper.Id = jobId;
                        jobWrapper.ControlId = control.Id;
                        jobWrapper.ScheduleRunId = scheduleRunId;
                        jobWrapper.Status = DHComputingJobStatus.Unknown;
                        jobWrapper = await monitoringService.CreateComputingJob(jobWrapper, payload.Operator).ConfigureAwait(false);


                        var dqJobId = string.Empty;
                        // Step 4: submit DQ jobs
                        // if (domainModelStatus == null)
                        // {
                        //     domainModelStatus = await dataQualityExecutionService.CheckDomainModelStatus(
                        //         requestHeaderContext.TenantId.ToString(),
                        //         requestHeaderContext.AccountObjectId.ToString());
                        // }
                        //
                        // if (domainModelStatus == DomainModelStatus.NoAccountMapping
                        //     || domainModelStatus == DomainModelStatus.NoSetup
                        //     || domainModelStatus == DomainModelStatus.NoData)
                        // {
                        //     logger.LogInformation($"Skip submit MDQ, DomainModelStatus: {domainModelStatus}, ControlId: {control.Id}. AssessmentId: {control.AssessmentId}");
                        //     // Update DQ status to skipped in monitoring table
                        //     jobWrapper.Status = DHComputingJobStatus.Skipped;
                        //     await monitoringService.UpdateComputingJob(jobWrapper, payload.Operator).ConfigureAwait(false);
                        //     logger.LogInformation($"Skip submit MDQ and update job status to skipped, DomainModelStatus: {domainModelStatus}, ControlId: {control.Id}. AssessmentId: {control.AssessmentId}");
                        //     continue;
                        // }

                        dqJobId = await dataQualityExecutionService.SubmitDQJob(
                            requestHeaderContext.TenantId.ToString(),
                            requestHeaderContext.AccountObjectId.ToString(),
                            control,
                            assessment,
                            jobId,
                            scheduleRunId,
                            isTriggeredFromGeneva).ConfigureAwait(false);

                        if (dqJobId.StartsWith("Skip-"))
                        {
                            jobWrapper.DQJobId = dqJobId.Replace("Skip-", "");
                            jobWrapper.Status = DHComputingJobStatus.Succeeded;
                            await monitoringService.UpdateComputingJob(jobWrapper, payload.Operator).ConfigureAwait(false);
                            logger.LogTipInformation($"The MDQ job is skipped", new JObject
                            {
                                { "jobId" , jobId },
                                { "dqJobId" , dqJobId },
                                { "controlId", control.Id },
                                { "controlName", control.Name},
                                { "scheduleRunId", scheduleRunId }
                            });
                            return scheduleRunId;
                        }

                        // Update DQ job id in monitoring table
                        jobWrapper.DQJobId = dqJobId;
                        jobWrapper.Status = DHComputingJobStatus.Created;
                        await monitoringService.UpdateComputingJob(jobWrapper, payload.Operator).ConfigureAwait(false);
                        logger.LogTipInformation($"The MDQ job was triggered", new JObject
                        {
                            { "jobId" , jobId },
                            { "dqJobId" , dqJobId },
                            { "controlId", control.Id },
                            { "controlName", control.Name},
                            { "scheduleRunId", scheduleRunId }
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"control failed to start. ControlId: {control.Id}. AssessmentId: {control.AssessmentId}", ex);
                        failedJobsCount++;
                    }
                }

                // trigger DQ control score batchly
                if (DQControlList.Count > 0)
                {
                    _ = this.UpdateDQScoreAsync(scheduleRunId, DQControlList);
                }

                if (failedJobsCount > 0)
                {
                    logger.LogTipInformation("Failed to trriger control job", new JObject { { "failedJobsCount", failedJobsCount } });
                }

                logger.LogInformation($"All MDQ jobs in schedule run {scheduleRunId} are created.");

                return scheduleRunId;
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to trigger schedule job", ex);
                throw;
            }
        }
    }

    private async Task<bool> skipIfNoBusinessDomainExists(string scheduleRunId)
    {
        bool hasDocuments = await dhDataEstateHealthRepository.DoesBusinessDomainHaveDocumentsAsync()
            .ConfigureAwait(false);

        if (!hasDocuments)
        {
            logger.LogInformation(
                $"No business domain documents found, skipping control processing for scheduled run {scheduleRunId}. " +
                $"TenantId: {requestHeaderContext.TenantId}, AccountId: {requestHeaderContext.AccountObjectId}");
        }

        return hasDocuments;
    }

    public async Task UpdateMDQJobStatusAsync(DHControlMDQJobCallbackPayload payload)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateMDQJobStatusAsync)}"))
        {
            var jobStatus = payload.ParseJobStatus();
            var job = await monitoringService.GetComputingJobByDQJobId(payload.DQJobId).ConfigureAwait(false);
            var tipInfo = new JObject
            {
                { "jobId" , job.Id },
                { "dqJobId", job.DQJobId },
                { "jobStatus", jobStatus.ToString() },
                { "controlId", job.ControlId },
            };
            logger.LogTipInformation("MDQ job status update", tipInfo);

            if (job.Status == DHComputingJobStatus.Failed)
            {
                logger.LogInformation($"Ignore failed job status update. Job ID: {job.Id}. Job status {jobStatus}.");
                await dataQualityExecutionService.PurgeObserver(job).ConfigureAwait(false);
                logger.LogInformation($"Purging Job from artifact store. Job ID: {job.Id}. Job status {jobStatus}.");
                return;
            }

            job.Status = jobStatus;

            if (jobStatus == DHComputingJobStatus.Succeeded)
            {
                try
                {
                    job.EndTime = DateTime.UtcNow;
                    await monitoringService.UpdateComputingJob(job, MDQEventOperationName).ConfigureAwait(false);
                    var scoreResult = await dataQualityExecutionService.ParseDQResult(job).ConfigureAwait(false);
                    await scoreService.ProcessControlComputingResultsAsync(job, scoreResult).ConfigureAwait(false);
                    await dataQualityExecutionService.PurgeObserver(job).ConfigureAwait(false);
                    logger.LogTipInformation("MDQ job score is processed successfully", tipInfo);
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception happened when processing succeed MDQ job.", ex);
                    throw;
                }
            }
            else
            {
                await monitoringService.UpdateComputingJob(job, MDQEventOperationName).ConfigureAwait(false);
                logger.LogTipInformation("MDQ job status update successfully", tipInfo);
            }
        }
    }

    private async Task UpdateDQGroupScoreAsync(string scheduleRunId, DHControlGroupWrapper controlGroup)
    {
        var jobId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        logger.LogInformation($"Starting to process DQ group score computing. ControlId: {controlGroup.Id}");
        try
        {
            // get all latest DQ score per BD/DP/Asset
            var scoreResult = await dataQualityScoreRepository.GetMultiple(
                new DataQualityScoreKey(requestHeaderContext.AccountObjectId),
                new System.Threading.CancellationToken()).ConfigureAwait(false);
            logger.LogInformation($"successfully fetched all scores: {scoreResult.Results.Count()}");
            // group all scores by DP
            var scores = scoreResult.Results.GroupBy(score => score.DataProductId).Select(group => new DHDataProductScoreWrapper()
            {
                ControlId = controlGroup.Id,
                ControlGroupId = controlGroup.Id,
                ScheduleRunId = scheduleRunId,
                Id = Guid.NewGuid().ToString(),
                ComputingJobId = jobId.ToString(),
                Time = now,
                Scores = group.Select(score => new DHScoreUnitWrapper()
                {
                    AssessmentRuleId = score.DataAssetId.ToString(),
                    Score = score.Score
                }),
                AggregatedScore = group.Average(score => score.Score),
                DataProductDomainId = group.First().BusinessDomainId.ToString(),
                DataProductId = group.First().DataProductId.ToString(),
                DataProductOwners = group.First().DataProductOwners,
                DataProductStatus = group.First().DataProductStatus
            }).ToList();
            await dhScoreRepository.AddAsync(scores).ConfigureAwait(false);
            logger.LogInformation($"successfully ingested all scores: {scores.Count()}");
        }
        catch (Exception ex)
        {
            logger.LogError("Exception happened when processing DQ score job.", ex);
        }
    }

    private async Task UpdateDQScoreAsync(string scheduleRunId, Dictionary<string, DHControlNodeWrapper> controlList)
    {
        var jobId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        logger.LogInformation($"Starting to process DQ score computing in batch.");
        try
        {
            // get all latest DQ score per BD/DP/Asset
            var scoreResult = await dataQualityScoreRepository.GetMultiple(
                new DataQualityScoreKey(requestHeaderContext.AccountObjectId, true),
                new System.Threading.CancellationToken()).ConfigureAwait(false);
            logger.LogInformation($"successfully fetched all scores: {scoreResult.Results.Count()}");

            foreach (var control in controlList)
            {
                logger.LogInformation($"start for dimension: {control.Key}");
                // group all scores by DP
                var scores = scoreResult.Results.Where(score => score.QualityDimension == control.Key).GroupBy(score => score.DataProductId).Select(group => new DHDataProductScoreWrapper()
                {
                    ControlId = control.Value.Id,
                    ControlGroupId = control.Value.GroupId,
                    ScheduleRunId = scheduleRunId,
                    Id = Guid.NewGuid().ToString(),
                    ComputingJobId = jobId.ToString(),
                    Time = now,
                    Scores = group.Select(score => new DHScoreUnitWrapper()
                    {
                        AssessmentRuleId = score.DataAssetId.ToString(),
                        Score = score.Score
                    }),
                    AggregatedScore = group.Average(score => score.Score),
                    DataProductDomainId = group.First().BusinessDomainId.ToString(),
                    DataProductId = group.First().DataProductId.ToString(),
                    DataProductOwners = group.First().DataProductOwners,
                    DataProductStatus = group.First().DataProductStatus
                }).ToList();
                await dhScoreRepository.AddAsync(scores).ConfigureAwait(false);
                logger.LogInformation($"successfully ingested all scores: {scores.Count()}. {control.Key}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Exception happened when processing DQ score job.", ex);
        }
    }

    public async Task MigrateScheduleAsync()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(MigrateScheduleAsync)}"))
        {
            var schedule = await this.GetGlobalScheduleInternalAsync();
            if (schedule == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), "Global"));
            }
            if (schedule.Host == DHControlScheduleHost.DGScheduleService)
            {
                await scheduleService.MigrateSchedule(schedule).ConfigureAwait(false);
                logger.LogInformation($"Migrate schedule successfully. Schedule id: {schedule.Id}");
            }
            else if (schedule.Host == DHControlScheduleHost.AzureStack)
            {
                logger.LogInformation($"Ignore schedule migrate. DEH Schedule job is already azure stack based. Schedule id: {schedule.Id}");
            }
        }
    }

    public async Task CreateGlobalScheduleInProvision()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateGlobalScheduleInProvision)}"))
        {
            var payload = new DHControlGlobalSchedulePayloadWrapper(new JObject());
            payload.Frequency = DHControlScheduleFrequency.Day;
            payload.Interval = 1;
            payload.Status = DHScheduleState.Enabled;

            // Trigger first schedule in some hour within next 24hrs.
            var random = new Random();
            var hours = random.Next(23) + 1;
            var time = DateTime.UtcNow.AddHours(hours);

            // Set JObject directly, otherwise entity validation fails.
            payload.JObject["startTime"] = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0, DateTimeKind.Utc);

            var schedule = await this.CreateOrUpdateGlobalScheduleAsync(payload).ConfigureAwait(false);
            logger.LogInformation($"Created a global schedule successfully with startTime {schedule.StartTime}.");
        }
    }

    public async Task<BatchResults<DHComputingJobWrapper>> ListMonitoringJobsByScheduleRunId(string scheduleRunId)
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(ListMonitoringJobsByScheduleRunId)}"))
        {
            logger.LogInformation($"List monitoring jobs by schedule run id {scheduleRunId}");
            var jobs = await monitoringService.QueryJobsWithScheduleRunId(scheduleRunId).ConfigureAwait(false);
            return jobs;
        }
    }

    public async Task<DHControlGlobalSchedulePayloadWrapper> CreateOrUpdateGlobalScheduleAsync(DHControlGlobalSchedulePayloadWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateOrUpdateGlobalScheduleAsync)}"))
        {
            entity.Validate();
            entity.NormalizeInput();

            var existingGlobalSchedule = await this.GetGlobalScheduleInternalAsync().ConfigureAwait(false);

            var scheduleStoragePayload = DHControlScheduleStoragePayloadWrapper.Create([]);
            scheduleStoragePayload.Properties = entity;
            scheduleStoragePayload.Type = DHControlScheduleType.ControlGlobal;

            if (existingGlobalSchedule == null)
            {
                // Create Global Schedule

                logger.LogInformation($"Creating Global Schedule");

                var result = await scheduleService.CreateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
                entity.SystemData = result.SystemData;

                logger.LogInformation($"Global schedule created successfully. {result.Id}");
                return entity;
            }
            else
            {
                // Update Global Schedule

                logger.LogInformation($"Updating Global Schedule {existingGlobalSchedule.Id}");

                scheduleStoragePayload.Id = existingGlobalSchedule.Id;
                var result = await scheduleService.UpdateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
                entity.SystemData = result.SystemData;
                return entity;
            }
        }

    }

    public async Task<DHControlGlobalSchedulePayloadWrapper> GetGlobalScheduleAsync()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(GetGlobalScheduleAsync)}"))
        {
            var globalSchedule = await this.GetGlobalScheduleInternalAsync().ConfigureAwait(false);

            if (globalSchedule == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), "Global"));
            }

            var response = new DHControlGlobalSchedulePayloadWrapper(globalSchedule.Properties.JObject);
            response.SystemData = globalSchedule.SystemData;

            return response;
        }
    }

    private async Task<DHControlScheduleStoragePayloadWrapper?> GetGlobalScheduleInternalAsync()
    {
        var globalScheduleQueryResult = await dhControlScheduleRepository.QueryScheduleAsync(DHControlScheduleType.ControlGlobal).ConfigureAwait(false);

        return globalScheduleQueryResult.FirstOrDefault();
    }

    public async Task<bool> GlobalScheduleExistsAsync()
    {
        using (logger.LogElapsed($"{this.GetType().Name}#{nameof(GlobalScheduleExistsAsync)}"))
        {
            var globalSchedule = await this.GetGlobalScheduleInternalAsync().ConfigureAwait(false);
            return globalSchedule != null;
        }
    }
}
