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
using Microsoft.Purview.DataEstateHealth.DHModels.Services;
using System.Collections.Generic;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Purview.DataEstateHealth.BusinessLogic.Interfaces;
using DEH.Domain.LogAnalytics;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataGovernance.Common;
using Newtonsoft.Json.Linq;
using IRequestHeaderContext = Azure.Purview.DataEstateHealth.Models.IRequestHeaderContext;

public class DHAnalyticsScheduleService : IDHAnalyticsScheduleService
{
    private readonly DHScheduleInternalService _scheduleService;
    private readonly IDataEstateHealthRequestLogger _logger;
    private readonly IDataQualityScoreRepository _dataQualityScoreRepository;
    private readonly DHAnalyticsScheduleRepository _dHAnalyticsScheduleRepository;
    private readonly Azure.Purview.DataEstateHealth.Models.IRequestHeaderContext _requestHeaderContext;
    private readonly DHDataEstateHealthRepository _dhDataEstateHealthRepository;
    private readonly IDEHAnalyticsJobLogsRepository dEHAnalyticsJobLogsRepository;
    private readonly DHMonitoringService _dhMonitoringService;
    private readonly DHAssessmentService _dhAssessmentService;
    private readonly DHControlService _dHControlService;
    private readonly IAccountExposureControlConfigProvider _exposureControl;


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
    DHDataEstateHealthRepository dhDataEstateHealthRepository,
    IDEHAnalyticsJobLogsRepository dEHAnalyticsJobLogsRepository,
    IAccountExposureControlConfigProvider exposureControl)
    {
        this._scheduleService = scheduleService;
        this._logger = logger;
        this._dataQualityScoreRepository = dataQualityScoreRepository;
        this._requestHeaderContext = requestHeaderContext;
        this._dHAnalyticsScheduleRepository = dHAnalyticsScheduleRepository;
        this._dhDataEstateHealthRepository = dhDataEstateHealthRepository;
        this.dEHAnalyticsJobLogsRepository = dEHAnalyticsJobLogsRepository;
        this._dHControlService = controlService;
        this._dhAssessmentService = assessmentService;
        this._dhMonitoringService = monitoringService;
        this._exposureControl = exposureControl;
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

                this._logger.LogInformation($"Creating Self Serve Analytics Global Schedule");

                var result = await this._scheduleService.CreateAnalyticsScheduleAsync(scheduleStoragePayload, null).ConfigureAwait(false);
                entity.SystemData = result.SystemData;

                this._logger.LogInformation($"Global Self Serve Analytics schedule created successfully. {result.Id}");
                return entity;
            }
            else
            {
                // Update Global Schedule

                this._logger.LogInformation($"Updating Global  Self Serve Analytics Schedule {existingGlobalSchedule.Id}");

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

    public Task<IReadOnlyList<DEHAnalyticsJobLogs>> GetDEHJobLogs(string accountId)
    {
        // Determine which table to use based on exposure control flag
        string tableName = "DEH_Job_Logs_CL";
        try
        {
            if (this._exposureControl?.IsEnableControlRedesignBillingImprovements(accountId, string.Empty, string.Empty) == true)
            {
                tableName = "DEH_Job_Logs_V2_CL";
            }
        }
        catch (Exception ex)
        {
            this._logger.LogWarning($"Failed to check exposure control flag, using default table: {ex.Message}");
        }

        // KQL query string to check for relevant job logs
        var kqlDehDq = $@"{tableName} 
	| where AccountId_g == '{accountId}'
	| where JobStatus_s !=""Started""
    | where JobName_s == ""StorageSync"" 
    | project JobId_g, JobStatus_s, ErrorMessage_s, JobCompletionTime_s
    | project-rename JobId=JobId_g, JobStatus=JobStatus_s, ErrorMessage=ErrorMessage_s, TimeStamp=JobCompletionTime_s";

        var result = this.dEHAnalyticsJobLogsRepository.GetDEHJobLogs(kqlDehDq, TimeSpan.FromDays(30));
        return result;

    }

}


