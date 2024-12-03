namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using global::Azure.Security.KeyVault.Secrets;
using LogAnalytics.Client;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Core.Services.JobManager.MetersToBillingJob.DTOs;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Models.ResourceModels.MDQJob;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.DomainModels;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter.Rules;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Exceptions;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using Microsoft.Purview.DataGovernance.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DataQualityExecutionService : IDataQualityExecutionService
{
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly DataQualityServiceClientFactory dataQualityServiceClientFactory;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IDataQualityOutputRepository dataQualityOutputRepository;

    private readonly LogAnalyticsClient logsAnalyticsWriter;
    private readonly IServiceProvider scope;
    private readonly IKeyVaultAccessorService keyVaultAccessorService;
    private readonly string keyVaultBaseURL;
    private string workspaceId;
    private string workspaceKey;
    private readonly LogAnalyticsManager.LogAnalyticsQueryClient logsAnalyticsReader;
    private readonly AzureCredentialFactory credentialFactory;

    public DataQualityExecutionService(
        IServiceProvider scope,
        IProcessingStorageManager processingStorageManager,
        DataQualityServiceClientFactory dataQualityServiceClientFactory,
        IDataQualityOutputRepository dataQualityOutputRepository,
        IDataEstateHealthRequestLogger logger)
    {
        this.processingStorageManager = processingStorageManager;
        this.dataQualityServiceClientFactory = dataQualityServiceClientFactory;
        this.logger = logger;
        this.dataQualityOutputRepository = dataQualityOutputRepository;

        this.credentialFactory = scope.GetService<AzureCredentialFactory>();
        this.keyVaultAccessorService = scope.GetService<IKeyVaultAccessorService>();
        var keyVaultConfig = scope.GetService<IOptions<KeyVaultConfiguration>>();
        this.keyVaultBaseURL = keyVaultConfig.Value.BaseUrl.ToString();
        this.scope = scope;
        this.keyVaultAccessorService = scope.GetService<IKeyVaultAccessorService>();
        // Get logAnalyticsWriter credentials
        var task = Task.Run(async () =>
        {
            await this.GetWorkspaceCredentials().ConfigureAwait(false);
        });
        Task.WaitAll(task);
        this.logsAnalyticsWriter = new LogAnalyticsClient(workspaceId: this.workspaceId, sharedKey: this.workspaceKey);
        // Log analytics reader
        LogAnalyticsManager manager = new LogAnalyticsManager(this.credentialFactory.CreateDefaultAzureCredential());
        this.logsAnalyticsReader = manager.WithWorkspace(this.workspaceId);
    }


    private async Task GetWorkspaceCredentials()
    {
        KeyVaultSecret workspaceId = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsWorkspaceId", default(CancellationToken)).ConfigureAwait(false);
        KeyVaultSecret workspaceKey = await this.keyVaultAccessorService.GetSecretAsync("logAnalyticsKey", default(CancellationToken)).ConfigureAwait(false);
        this.workspaceId = workspaceId.Value;
        this.workspaceKey = workspaceKey.Value;
    }

    public async Task<IEnumerable<DHRawScore>> ParseDQResult(DHComputingJobWrapper job)
    {
        try
        {
            this.logger.LogInformation($"Start ParseDQResult, accountId:{job.AccountId}, controlId:{job.ControlId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");

            // Query storage account
            var accountStorageModel = await this.processingStorageManager.Get(new Guid(job.AccountId), CancellationToken.None).ConfigureAwait(false);
            if (accountStorageModel == null)
            {
                this.logger.LogInformation($"Interrupt ParseDQResult, processing storage account mapping does not exist, tenantId:{job.TenantId}, accountId:{job.AccountId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");
                throw new ProcessingStorageAccountMappingNotExistsException();
            }

            var dataProductId = job.ControlId;
            var dataAssetId = job.Id;

            var outputResult = await this.dataQualityOutputRepository.GetMultiple(new DataQualityOutputQueryCriteria()
            {
                AccountStorageModel = accountStorageModel,
                FolderPath = ErrorOutputInfo.GeneratePartOfFolderPath(dataProductId, dataAssetId) + $"/observation={job.DQJobId}"
            }, CancellationToken.None).ConfigureAwait(false);

            this.logger.LogInformation($"Read output is done, row count:{outputResult.Results.Count()}, healthJobId:{job.Id}");

            var result = DataQualityOutputAdapter.ToScorePayload(outputResult.Results, this.logger);

            this.logger.LogInformation($"End ParseDQResult, resultCount:{result.Count()}, healthJobId:{job.Id}");
            if (result.Count() > 0)
            {
                this.logger.LogInformation($"End ParseDQResult, parsedRuleCount:{result.First().Scores.Count()}, healthJobId:{job.Id}");
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new ParseMDQResultException(ex.Message, ex);
        }
    }

    public async Task<string> SubmitDQJob(string tenantId, string accountId, DHControlNodeWrapper control, DHAssessmentWrapper assessment, string healthJobId, string scheduleRunId, bool isTriggeredFromGeneva)
    {
        try
        {
            if (!isTriggeredFromGeneva)
            {
                var isControlRanInLast12Hours = await this.IsControlRanInLast12Hour(accountId, control.Id).ConfigureAwait(false);
                if (isControlRanInLast12Hours)
                {
                    this.logger.LogInformation($"Skipping SubmitDQJob as the control has run in the last 12 hours, tenantId:{tenantId}, accountId:{accountId}, controlId:{control.Id}");
                    return $"Skip-{new Guid().ToString()}";
                }
            }
            
            this.logger.LogInformation($"Start SubmitDQJOb, tenantId:{tenantId}, accountId:{accountId}, controlId:{control.Id}, healthJobId:{healthJobId}");

            // Query storage account
            var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);

            var dfsEndpoint = accountStorageModel.GetDfsEndpoint();
            var catalogId = accountStorageModel.CatalogId.ToString();

            this.logger.LogInformation($"Found storage account, dfsEndpoint:{dfsEndpoint}, catalogId:{catalogId}");

            var dataProductId = control.Id;
            var dataAssetId = healthJobId;

            // Convert to an observer
            var adaptContext = new RuleAdapterContext(
                dfsEndpoint,
                catalogId,
                dataProductId,
                dataAssetId,
                assessment,
                control.Domains);
            var observerAdapter = new ObserverAdapter(adaptContext);
            var observer = observerAdapter.FromControlAssessment();

            observer.ExecutionData = new JObject()
            {
                { DataEstateHealthConstants.DEH_KEY_DATA_SOURCE_ENDPOINT, dfsEndpoint }
            };

            var dataQualityServiceClient = this.dataQualityServiceClientFactory.GetClient();

            // For debug
            var observerPayload = JsonConvert.SerializeObject(observer.JObject);
            // TODO will delete this log after everything is stable
            this.logger.LogInformation($"Observer payload: {observerPayload}, healthJobId:{healthJobId}");

            // Create a temporary observer
            await dataQualityServiceClient.CreateObserver(observer, tenantId, accountId).ConfigureAwait(false);

            // Trigger run
            var aliasList = observer.InputDatasets.Select(inputDataset => inputDataset.Alias).ToList();

            var dqJobId = await dataQualityServiceClient.TriggerJobRun(
                tenantId,
                accountId,
                dataProductId,
                dataAssetId,
                new JobSubmitPayload(
                    dfsEndpoint,
                    catalogId,
                    dataProductId,
                    dataAssetId,
                    aliasList)).ConfigureAwait(false);

            await this.LogDQJobInitMappingJob(dqJobId, scheduleRunId, tenantId, accountId, control.Id, control.Name).ConfigureAwait(false);

            this.logger.LogInformation($"End SubmitDQJOb, controlId: {control.Id}, healthJobId: {healthJobId}, dqJobId:{dqJobId}");

            return dqJobId;
        }
        catch (Exception ex)
        {
            throw new MDQJobDQSubmissionException(ex.Message, ex);
        }
    }

    private async Task<bool> IsControlRanInLast12Hour(string accountId, string controlId)
    {
        // Define time window for query
        var fromDate = DateTimeOffset.UtcNow.AddHours(-12);
        var toDate = DateTimeOffset.UtcNow;

        // TODO: Remove this after one week - temporary solution to process the queue for these accounts
        List<string> accountIds = new List<string> 
        {   "268675e1-07b2-4ee0-a752-0d54921cd84b",
            "0c89ab95-b9d6-47d8-9101-913ded037ba6",
            "3f97dbec-dcf4-4b96-b81f-29929c32d93c",
            "f0029967-80f0-4dd5-9df1-cd17b8b24a67"
        };
        if (accountIds.Contains(accountId)) {
            fromDate = DateTimeOffset.UtcNow.AddDays(-90);
        }

        // KQL query string to check for relevant job logs
        var kqlDehDq =  $@"DEH_JobInitMapping_log_CL
                            | where AccountId_g == ""{accountId}"" and ControlId_g == ""{controlId}""
                            | limit 1";
        try
        {
            // Execute the query and fetch events
            var dehEvents = await this.logsAnalyticsReader.Query<DQJobMappingLogTable>(kqlDehDq, fromDate, toDate).ConfigureAwait(false);

            // If no events were found, return false
            if (dehEvents?.Value?.Count == 0)
            {
                this.logger.LogInformation($"No control Job found within the last 12 hours for account: {accountId}");
                return false;
            }
            foreach (var dehEvent in dehEvents.Value)
            {
                this.logger.LogInformation($"Control Job found: {dehEvent}");
            }
            return true;
        }
        catch (Exception ex)
        {
            // Log error if the query or other process fails
            this.logger.LogError($"Failed to get Control Job Events from logs for account {accountId}: {ex.Message}", ex);
            return true; // Consider returning `true` as a default to ensure job submission attempt
        }
    }

    private async Task LogDQJobInitMappingJob(string dqJobId, string batchId, string tenantId, string accountId, string controlId, string controlName)
    {
        try
        {
            string dqLogTableName = "DEH_JobInitMapping_log";
            List<DQJobMappingLogTable> billingEvents = new List<DQJobMappingLogTable>();
            DQJobMappingLogTable dqJobMappingLogTable = new DQJobMappingLogTable()
            {
                AccountId = Guid.Parse(accountId),                
                PurviewTenantId = Guid.Parse(tenantId),
                BatchId = batchId,
                CreatedAt = DateTime.UtcNow,
                DQJobId = Guid.Parse(dqJobId),
                JobStatus = "Init",
                ControlId = controlId,
                ControlName = controlName
            };
            billingEvents.Add(dqJobMappingLogTable);
            var dqJobMapping = billingEvents.ToList();
            //Create the table if it does not exist
            await this.logsAnalyticsWriter.SendLogEntries<DQJobMappingLogTable>(dqJobMapping, dqLogTableName).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.logger.LogError("MDQ|SubmitDQJob|Unable to create Log table DEH_JobMapping_log -> MDQ job", ex);
        }
    }

    public async Task<DomainModelStatus> CheckDomainModelStatus(string tenantId, string accountId)
    {
        // Query storage account
        var accountStorageModel = await this.processingStorageManager.Get(new Guid(accountId), CancellationToken.None).ConfigureAwait(false);

        if (accountStorageModel == null)
        {
            return DomainModelStatus.NoAccountMapping;
        }

        // Check if domain model exists
        var domainModelStatus = await this.processingStorageManager.CheckDomainModelExists(
            accountStorageModel,
            // Use BusinessDomain folder as representative to check
            DomainModelUtils.GetDomainModel(DomainModelType.BusinessDomain).FolderPath).ConfigureAwait(false);
        return domainModelStatus;
    }

    public async Task PurgeObserver(DHComputingJobWrapper job)
    {
        try
        {
            this.logger.LogInformation($"Start PurgeObserver, accountId:{job.AccountId}, controlId:{job.ControlId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");
            var dataQualityServiceClient = this.dataQualityServiceClientFactory.GetClient();
            await dataQualityServiceClient.DeleteObserver(job.TenantId, job.AccountId, job.ControlId, job.Id).ConfigureAwait(false);
            this.logger.LogInformation($"End PurgeObserver, accountId:{job.AccountId}, controlId:{job.ControlId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");
        }
        catch (Exception ex)
        {
            throw new PurgeObserverException(ex.Message, ex);
        }
    }
}
