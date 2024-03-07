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
    private const string ScheduleOperationName = "DGSchedule Service";
    private const string MDQEventOperationName = "MDQ event";

    public async Task TriggerScheduleAsync(DHScheduleCallbackPayload payload)
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

        foreach (var control in controls)
        {
            if (control.Status == DHControlStatus.Disabled)
            {
                logger.LogInformation($"control is disabled, skip. ControlId: {payload.ControlId}.");
                continue;
            }

            // Step 2: submit DQ jobs
            var assessment = await assessmentService.GetAssessmentByIdAsync(control.AssessmentId!).ConfigureAwait(false);
            if (assessment.TargetQualityType == DHAssessmentQualityType.DataQuality)
            {
                _ = this.UpdateDQScoreAsync(control);
                continue;
            }
            var jobId = Guid.NewGuid().ToString();
            var dqJobId = await dataQualityExecutionService.SubmitDQJob(
                requestHeaderContext.TenantId.ToString(),
                requestHeaderContext.AccountObjectId.ToString(),
                control,
                assessment,
                jobId).ConfigureAwait(false);

            // Step 3: save into monitoring table
            var jobWrapper = new DHComputingJobWrapper();
            jobWrapper.Id = jobId;
            jobWrapper.DQJobId = dqJobId;
            jobWrapper.ControlId = control.Id;
            await monitoringService.CreateComputingJob(jobWrapper, ScheduleOperationName).ConfigureAwait(false);
            logger.LogInformation($"New MDQ job created. Job Id: {jobId}. DQ job Id: {dqJobId}. Control Id:{control.Id}");
            logger.LogTipInformation($"The MDQ job was triggered", new JObject
            {
                { "jobId" , dqJobId },
                { "controlId", control.Id },
                { "controlName", control.Name}
            });
        }
    }

    public async Task UpdateMDQJobStatusAsync(DHControlMDQJobCallbackPayload payload)
    {
        var jobStatus = payload.ParseJobStatus();
        var job = await monitoringService.GetComputingJobByDQJobId(payload.DQJobId).ConfigureAwait(false);
        await monitoringService.UpdateComputingJobStatus(job.Id, jobStatus, MDQEventOperationName);
        logger.LogInformation($"MDQ job status updated: {jobStatus}. Job Id: {job.Id}. DQ Job Id: {job.DQJobId}.");

        if (jobStatus == DHComputingJobStatus.Succeeded)
        {
            logger.LogInformation($"New succeed MDQ job. Process score computing. Job Id: {job.Id}. DQ job Id: {job.DQJobId}. Control Id: {job.ControlId}");
            logger.LogTipInformation($"The MDQ job was finished successfully", new JObject
            {
                { "jobId" , job.DQJobId },
                { "controlId", job.ControlId },
            });
            try
            {
                await monitoringService.EndComputingJob(job.Id, MDQEventOperationName);
                // TODO: fulfill dataProductId, dataAssetId, jobId
                var scoreResult = await dataQualityExecutionService.ParseDQResult(job.AccountId, job.ControlId, job.Id, job.DQJobId).ConfigureAwait(false);
                await scoreService.ProcessControlComputingResultsAsync(job.ControlId, job.Id, scoreResult).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError("Exception happened when processing succeed MDQ job.", ex);
                throw;
            }
        }
    }
    public async Task UpdateDQScoreAsync(DHControlNodeWrapper control)
    {
        var jobId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        logger.LogInformation($"Starting to process DQ score computing. ControlId: {control.Id}, Name: {control.Name}");
        try
        {
            // get all latest DQ score per BD/DP/Asset
            var scoreResult = await dataQualityScoreRepository.GetMultiple(new DataQualityScoreKey(requestHeaderContext.AccountObjectId), new System.Threading.CancellationToken()).ConfigureAwait(false);
            logger.LogInformation($"successfully fetched all scores: {scoreResult.Results.Count()}");
            // group all scores by DP
            var scores = scoreResult.Results.GroupBy(score => score.DataProductId).Select(group => new DHDataProductScoreWrapper()
            {
                ControlId = control.Id.ToString(),
                ControlGroupId = control.GroupId.ToString(),
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
                DataProductOwners = group.First().DataProductOwners
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

        entity.Validate();
        entity.NormalizeInput();

        var existingGlobalSchedule = await this.GetGlobalScheduleInternalAsync().ConfigureAwait(false);

        var scheduleStoragePayload = DHControlScheduleStoragePayloadWrapper.Create([]);
        scheduleStoragePayload.Properties = entity;
        scheduleStoragePayload.Type = DHControlScheduleType.ControlGlobal;

        if (existingGlobalSchedule == null)
        {
            // Create Global Schedule
            var result = await scheduleService.CreateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
            entity.SystemData = result.SystemData;
            return entity;
        }
        else
        {
            // Update Global Schedule
            scheduleStoragePayload.Id = existingGlobalSchedule.Id;
            var result = await scheduleService.UpdateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
            entity.SystemData = result.SystemData;
            return entity;
        }
    }

    public async Task<DHControlGlobalSchedulePayloadWrapper> GetGlobalScheduleAsync()
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

    private async Task<DHControlScheduleStoragePayloadWrapper?> GetGlobalScheduleInternalAsync()
    {
        var globalScheduleQueryResult = await dhControlScheduleRepository.QueryScheduleAsync(DHControlScheduleType.ControlGlobal).ConfigureAwait(false);

        return globalScheduleQueryResult.FirstOrDefault();
    }
}
