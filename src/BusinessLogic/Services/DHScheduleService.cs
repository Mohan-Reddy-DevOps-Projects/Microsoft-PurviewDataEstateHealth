#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
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
    DHScoreRepository dhScoreRepository
    )
{
    private const string MDQEventOperationName = "MDQ event";

    public async Task<string> TriggerScheduleJobCallbackAsync(DHScheduleCallbackPayload payload)
    {
        try
        {
            // Step 1: query all controls
            var controls = new List<DHControlNodeWrapper>();

            if (string.IsNullOrEmpty(payload.ControlId))
            {
                var result = await controlService.ListControlsAsync().ConfigureAwait(false);
                var controlNodes = result.Results.Where(item => item is DHControlNodeWrapper).OfType<DHControlNodeWrapper>();
                controls.AddRange(controlNodes);
                logger.LogInformation($"Trigger batch controls jobs. Count {controls.Count}.");
            }
            else
            {
                var result = await controlService.GetControlByIdAsync(payload.ControlId).ConfigureAwait(false);
                controls.Add((DHControlNodeWrapper)result);
                logger.LogInformation($"Trigger control job. ControlId {payload.ControlId}.");
            }

            var scheduleRunId = Guid.NewGuid().ToString();
            logger.LogInformation($"New scheduleRunId generated {scheduleRunId}");

            var assessments = await assessmentService.ListAssessmentsAsync().ConfigureAwait(false);
            int failedJobsCount = 0;
            foreach (var control in controls)
            {
                try
                {
                    logger.LogInformation($"start with control, Id: {control.Id}. AssessmentId: {control.AssessmentId}");
                    if (control.Status != DHControlStatus.Enabled)
                    {
                        logger.LogInformation($"control is not enabled, skip. ControlId: {control.Id}. AssessmentId: {control.AssessmentId}");
                        continue;
                    }

                    var assessment = assessments.Results.First(item => item.Id == control.AssessmentId);
                    if (assessment.TargetQualityType == DHAssessmentQualityType.DataQuality)
                    {
                        _ = this.UpdateDQScoreAsync(scheduleRunId, control, assessment);
                        continue;
                    }

                    if (assessment.Rules == null || !assessment.Rules.Any())
                    {
                        logger.LogInformation($"Control has no rules, skip. ControlId: {control.Id}. AssessmentId: {control.AssessmentId}");
                        continue;
                    }

                    // Step 2: save into monitoring table
                    var jobId = Guid.NewGuid().ToString();
                    var jobWrapper = new DHComputingJobWrapper();
                    jobWrapper.Id = jobId;
                    jobWrapper.ControlId = control.Id;
                    jobWrapper.ScheduleRunId = scheduleRunId;
                    jobWrapper.Status = DHComputingJobStatus.Unknown;
                    jobWrapper = await monitoringService.CreateComputingJob(jobWrapper, payload.Operator).ConfigureAwait(false);

                    // Step 3: submit DQ jobs
                    var dqJobId = await dataQualityExecutionService.SubmitDQJob(
                        requestHeaderContext.TenantId.ToString(),
                        requestHeaderContext.AccountObjectId.ToString(),
                        control,
                        assessment,
                        jobId).ConfigureAwait(false);

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
                    logger.LogCritical($"control failed to start. ControlId: {control.Id}. AssessmentId: {control.AssessmentId}", ex);
                    failedJobsCount++;
                }
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
            logger.LogCritical($"Failed to trigger schedule job", ex);
            throw;
        }

    }

    public async Task UpdateMDQJobStatusAsync(DHControlMDQJobCallbackPayload payload)
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
    public async Task UpdateDQScoreAsync(string scheduleRunId, DHControlNodeWrapper control, DHAssessmentWrapper assessment)
    {
        var jobId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var dimension = (assessment.Rules.First()?.Rule as DHSimpleRuleWrapper)?.CheckPoint;
        logger.LogInformation($"Starting to process DQ score computing. ControlId: {control.Id}, Name: {control.Name}, Dimenstion: {dimension}");
        try
        {
            // get all latest DQ score per BD/DP/Asset
            var scoreResult = await dataQualityScoreRepository.GetMultiple(
                new DataQualityScoreKey(requestHeaderContext.AccountObjectId, DQDimentionConvert.ConvertCheckPointToDQDimension(dimension)),
                new System.Threading.CancellationToken()).ConfigureAwait(false);
            logger.LogInformation($"successfully fetched all scores: {scoreResult.Results.Count()}");
            // group all scores by DP
            var scores = scoreResult.Results.GroupBy(score => score.DataProductId).Select(group => new DHDataProductScoreWrapper()
            {
                ControlId = control.Id.ToString(),
                ControlGroupId = control.GroupId.ToString(),
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
}
