// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.SparkJobs.CatalogSparkJob;

using MediatR;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataGovernance.Common.Error;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using System.Threading;
using System.Threading.Tasks;

internal class TriggerMdqRulesSpecificationStage : IJobCallbackStage
{
    private readonly JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils;
    private readonly DehScheduleJobMetadata metadata;
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;
    private readonly IDataHealthApiService dataHealthApiService;

    public TriggerMdqRulesSpecificationStage(
        IServiceScope scope,
        DehScheduleJobMetadata metadata,
        JobCallbackUtils<DehScheduleJobMetadata> jobCallbackUtils)
    {
        this.metadata = metadata;
        this.jobCallbackUtils = jobCallbackUtils;
        this.dataEstateHealthRequestLogger = scope.ServiceProvider.GetService<IDataEstateHealthRequestLogger>();
        scope.ServiceProvider.GetService<ISender>();
        scope.ServiceProvider.GetService<IAccountExposureControlConfigProvider>();
        this.dataHealthApiService = scope.ServiceProvider.GetService<IDataHealthApiService>();
    }

    public string StageName => nameof(TriggerMdqRulesSpecificationStage);

    public async Task<JobExecutionResult> Execute()
    {
        string accountId = this.metadata.CatalogSparkJobMetadata.AccountServiceModel.Id;
        string tenantId = this.metadata.CatalogSparkJobMetadata.AccountServiceModel.TenantId;
        
        using (this.dataEstateHealthRequestLogger.LogElapsed($"Start to create data quality rules specification"))
        {
            JobExecutionStatus jobStageStatus;
            string jobStatusMessage;
            try
            {
                this.dataEstateHealthRequestLogger.LogInformation($"Creating data quality rules specification for account: {accountId}, tenant: {tenantId}");
                
                // Create data quality rules specification
                var dataQualitySpecResult = await this.dataHealthApiService.CreateDataQualityRulesSpec(
                    new TriggeredSchedulePayload()
                    {
                        AccountId = accountId,
                        TenantId = tenantId,
                        RequestId = this.metadata.CatalogSparkJobMetadata.TraceId,
                        Operator = DHScheduleCallbackPayload.DGScheduleServiceOperatorName,
                        TriggerType = nameof(DHScheduleCallbackTriggerType.Schedule),
                    }, CancellationToken.None);
                
                if (!dataQualitySpecResult.Success)
                {
                    throw new ServiceException($"Failed to create data quality rules specification for account: {accountId}");
                }
                
                this.dataEstateHealthRequestLogger.LogInformation($"Successfully created data quality rules specification for account: {accountId}, Workflow Run ID: {dataQualitySpecResult.ControlsWorkflowId}");
                
                this.metadata.SqlGenerationMetadata.SqlGenerationStatus = SqlGenerationWorkflowStatus.Succeeded;
                this.metadata.SqlGenerationMetadata.ControlsWorkflowJobRunId = dataQualitySpecResult.ControlsWorkflowId;
                jobStageStatus = JobExecutionStatus.Completed;
                jobStatusMessage = $"Data quality rules specification creation completed for account: {accountId}, Workflow Run ID: {dataQualitySpecResult.ControlsWorkflowId}";
                this.dataEstateHealthRequestLogger.LogInformation(jobStatusMessage);
            }
            catch (Exception exception)
            {
                this.metadata.SqlGenerationMetadata.SqlGenerationStatus = SqlGenerationWorkflowStatus.Failed;
                jobStageStatus = JobExecutionStatus.Failed;
                jobStatusMessage = $"Failed to create data quality rules specification for account: {accountId} with error: {exception.Message}";
                this.dataEstateHealthRequestLogger.LogError(jobStatusMessage, exception);
            }

            return this.jobCallbackUtils.GetExecutionResult(jobStageStatus, jobStatusMessage, DateTime.UtcNow.Add(TimeSpan.FromSeconds(10)));
        }
    }

    public bool IsStageComplete()
    {
        // Check if data quality rules specification creation succeeded
        return this.metadata.SqlGenerationMetadata.SqlGenerationStatus == SqlGenerationWorkflowStatus.Succeeded;
    }

    public bool IsStagePreconditionMet()
    {
        // This stage should execute to create data quality rules specification
        return true;
    }
} 