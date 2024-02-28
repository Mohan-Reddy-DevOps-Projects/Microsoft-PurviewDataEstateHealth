#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
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
    IDataEstateHealthRequestLogger logger
    )
{
    public async Task TriggerScheduleAsync(DHScheduleCallbackPayload payload)
    {
        // Step 1: query all controls
        var controls = new List<DHControlBaseWrapper>();

        if (string.IsNullOrEmpty(payload.ControlId))
        {
            var result = await controlService.ListControlsAsync().ConfigureAwait(false);
            controls.AddRange(result.Results);
            logger.LogInformation($"Trigger batch controls jobs. Count {result.Count}.");
        }
        else
        {
            var result = await controlService.GetControlByIdAsync(payload.ControlId).ConfigureAwait(false);
            controls.Add(result);
            logger.LogInformation($"Trigger control job. ControlId {payload.ControlId}.");
        }

        foreach (var control in controls)
        {
            // Step 2: submit DQ jobs
            var jobId = Guid.NewGuid().ToString();
            var dqJobId = await dataQualityExecutionService.SubmitDQJob(payload.AccountId, control.Id, jobId).ConfigureAwait(false);

            // Step 3: save into monitoring table
            var jobWrapper = new DHComputingJobWrapper();
            jobWrapper.Id = jobId;
            jobWrapper.DQJobId = dqJobId;
            jobWrapper.ControlId = control.Id;
            await monitoringService.CreateComputingJob(jobWrapper).ConfigureAwait(false);
        }
    }

    public async Task UpdateMDQJobStatusAsync(DHControlMDQJobCallbackPayload payload)
    {
        var jobStatus = payload.ParseJobStatus();
        var job = await monitoringService.GetComputingJobByDQJobId(payload.DQJobId).ConfigureAwait(false);
        if (job.Status == jobStatus)
        {
            logger.LogInformation($"MDQ job status has already been {jobStatus}. Ignore update. Job ID: {payload.DQJobId}");
            return;
        }
        await monitoringService.UpdateComputingJobStatus(job.Id, jobStatus);
        logger.LogInformation($"MDQ job status updated: {jobStatus}. Job Id: {job.Id}. DQ Job Id: {job.DQJobId}.");

        if (jobStatus == DHComputingJobStatus.Succeeded)
        {
            logger.LogInformation($"New succeed MDQ job. Process score computing. Job Id: {job.Id}. DQ Job Id: {job.DQJobId}. Control Id: {job.ControlId}");

            try
            {
                await monitoringService.EndComputingJob(job.Id);
                // TODO: fulfill dataProductId, dataAssetId, jobId
                var scoreResult = await dataQualityExecutionService.ParseDQResult(job.AccountId, job.ControlId, job.Id, job.DQJobId).ConfigureAwait(false);
                await scoreService.ProcessControlComputingResultsAsync(job.ControlId, job.Id, scoreResult).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError("Exception happened when processing succeed MDQ job.", ex);
            }
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
            entity.AuditLogs = result.AuditLogs;
            return entity;
        }
        else
        {
            // Update Global Schedule
            scheduleStoragePayload.Id = existingGlobalSchedule.Id;
            var result = await scheduleService.UpdateScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
            entity.AuditLogs = result.AuditLogs;
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
        response.AuditLogs = globalSchedule.AuditLogs;

        return response;
    }

    private async Task<DHControlScheduleStoragePayloadWrapper?> GetGlobalScheduleInternalAsync()
    {
        var globalScheduleQueryResult = await dhControlScheduleRepository.QueryScheduleAsync(DHControlScheduleType.ControlGlobal).ConfigureAwait(false);

        return globalScheduleQueryResult.FirstOrDefault();
    }
}
