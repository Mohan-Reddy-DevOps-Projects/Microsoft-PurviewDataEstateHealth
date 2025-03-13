#nullable enable

namespace Microsoft.Purview.DataEstateHealth.BusinessLogic.Services;

using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions.Model;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Exceptions;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.InternalServices;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Attributes;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Interfaces;
using Microsoft.Azure.Management.Storage.Models;

public class DHAnalyticsScheduleService : IDHAnalyticsScheduleService
{
    private readonly DHScheduleInternalService _scheduleService;
    private readonly IDataQualityExecutionService _dataQualityExecutionService;
    private readonly DHMonitoringService _monitoringService;
    private readonly DHControlService _controlService;
    private readonly DHAssessmentService _assessmentService;
    private readonly DHScoreRepository _dhScoreRepository;
    private readonly IDataEstateHealthRequestLogger _logger;
    private readonly IDataQualityScoreRepository _dataQualityScoreRepository;
    private readonly IRequestHeaderContext _requestHeaderContext;
    private readonly DHAnalyticsScheduleRepository _dHAnalyticsScheduleRepository;
    private readonly DHDataEstateHealthRepository _dhDataEstateHealthRepository;

    public DHAnalyticsScheduleService(
    DHScheduleInternalService scheduleService,
    IDataQualityExecutionService dataQualityExecutionService,
    DHMonitoringService monitoringService,
    DHControlService controlService,
    DHAssessmentService assessmentService,
     DHScoreRepository dhScoreRepository,
    IDataEstateHealthRequestLogger logger,
    IDataQualityScoreRepository dataQualityScoreRepository,
    IRequestHeaderContext requestHeaderContext,
     DHAnalyticsScheduleRepository dHAnalyticsScheduleRepository,
      DHDataEstateHealthRepository dhDataEstateHealthRepository)
    {
        this._scheduleService = scheduleService;
        this._dataQualityExecutionService = dataQualityExecutionService;
        this._monitoringService = monitoringService;
        this._controlService = controlService;
        this._assessmentService = assessmentService;
        this._dhScoreRepository = dhScoreRepository;
        this._logger = logger;
        this._dataQualityScoreRepository = dataQualityScoreRepository;
        this._requestHeaderContext = requestHeaderContext;
        this._dHAnalyticsScheduleRepository = dHAnalyticsScheduleRepository;
        this._dhDataEstateHealthRepository = dhDataEstateHealthRepository;

    }
    public async Task<DHControlGlobalSchedulePayloadWrapper> CreateOrUpdateAnalyticsGlobalScheduleAsync(DHControlGlobalSchedulePayloadWrapper entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        using (this._logger.LogElapsed($"{this.GetType().Name}#{nameof(CreateOrUpdateAnalyticsGlobalScheduleAsync)}"))
        {
            entity.Validate();
            entity.NormalizeInput();

            var existingGlobalSchedule = await this.GetAnalyticsGlobalScheduleInternalAsync().ConfigureAwait(false);

            var scheduleStoragePayload = DHControlScheduleStoragePayloadWrapper.Create([]);
            scheduleStoragePayload.Properties = entity;
            scheduleStoragePayload.Type = DHControlScheduleType.ControlGlobal;

            if (existingGlobalSchedule == null)
            {
                // Create Global Schedule

                this._logger.LogInformation($"Creating Global Schedule");

                var result = await this._scheduleService.CreateAnalyticsScheduleAsync(scheduleStoragePayload, null).ConfigureAwait(false);
                entity.SystemData = result.SystemData;

                this._logger.LogInformation($"Global schedule created successfully. {result.Id}");
                return entity;
            }
            else
            {
                // Update Global Schedule

                this._logger.LogInformation($"Updating Global Schedule {existingGlobalSchedule.Id}");

                scheduleStoragePayload.Id = existingGlobalSchedule.Id;
                var result = await this._scheduleService.UpdateAnalyticsScheduleAsync(scheduleStoragePayload).ConfigureAwait(false);
                entity.SystemData = result.SystemData;
                return entity;
            }
        }

    }

    public async Task<DHControlGlobalSchedulePayloadWrapper> GetAnalyticsGlobalScheduleAsync()
    {
        using (this._logger.LogElapsed($"{this.GetType().Name}#{nameof(GetAnalyticsGlobalScheduleAsync)}"))
        {
            var globalSchedule = await this.GetAnalyticsGlobalScheduleInternalAsync().ConfigureAwait(false);

            if (globalSchedule == null)
            {
                throw new EntityNotFoundException(new ExceptionRefEntityInfo(EntityCategory.Schedule.ToString(), "Global"));
            }

            var response = new DHControlGlobalSchedulePayloadWrapper(globalSchedule.Properties.JObject);
            response.SystemData = globalSchedule.SystemData;

            return response;
        }
    }

    private async Task<DHControlScheduleStoragePayloadWrapper?> GetAnalyticsGlobalScheduleInternalAsync()
    {
        var globalScheduleQueryResult = await this._dHAnalyticsScheduleRepository.QueryAnalyticsScheduleAsync(DHControlScheduleType.ControlGlobal).ConfigureAwait(false);

        return globalScheduleQueryResult.FirstOrDefault();
    }

    public async Task<string> TriggerAnalyticsScheduleJobCallbackAsync(DHAnalyticsScheduleCallbackPayload payload, bool isTriggeredFromGeneva = false)
    {
        using (this._logger.LogElapsed($"{this.GetType().Name}#{nameof(TriggerAnalyticsScheduleJobCallbackAsync)}"))
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

                return scheduleRunId;
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Failed to trigger schedule job", ex);
                throw;
            }
        }
    }

    private async Task<bool> skipIfNoBusinessDomainExists(string scheduleRunId)
    {
        bool hasDocuments = await this._dhDataEstateHealthRepository.DoesBusinessDomainHaveDocumentsAsync()
            .ConfigureAwait(false);

        if (!hasDocuments)
        {
            this._logger.LogInformation(
                $"No business domain documents found, skipping control processing for scheduled run {scheduleRunId}. " +
                $"TenantId: {this._requestHeaderContext.TenantId}, AccountId: {this._requestHeaderContext.AccountObjectId}");
        }

        return hasDocuments;
    }


}


