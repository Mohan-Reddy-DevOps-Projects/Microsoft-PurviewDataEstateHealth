namespace Microsoft.Purview.DataEstateHealth.DHModels.Services;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess.Repositories.DataQualityOutput;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters;
using Microsoft.Purview.DataEstateHealth.DHModels.Adapters.RuleAdapter;
using Microsoft.Purview.DataEstateHealth.DHModels.Constants;
using Microsoft.Purview.DataEstateHealth.DHModels.Models;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.DHAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
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

    public DataQualityExecutionService(
        IProcessingStorageManager processingStorageManager,
        DataQualityServiceClientFactory dataQualityServiceClientFactory,
        IDataQualityOutputRepository dataQualityOutputRepository,
        IDataEstateHealthRequestLogger logger)
    {
        this.processingStorageManager = processingStorageManager;
        this.dataQualityServiceClientFactory = dataQualityServiceClientFactory;
        this.logger = logger;
        this.dataQualityOutputRepository = dataQualityOutputRepository;
    }

    public async Task<IEnumerable<DHRawScore>> ParseDQResult(DHComputingJobWrapper job)
    {
        this.logger.LogInformation($"Start ParseDQResult, accountId:{job.AccountId}, controlId:{job.ControlId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");

        var dataProductId = job.ControlId;
        var dataAssetId = job.Id;

        var outputResult = await this.dataQualityOutputRepository.GetMultiple(new DataQualityOutputQueryCriteria()
        {
            AccountId = job.AccountId,
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

    public async Task<string> SubmitDQJob(string tenantId, string accountId, DHControlNodeWrapper control, DHAssessmentWrapper assessment, string healthJobId)
    {
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

        this.logger.LogInformation($"End SubmitDQJOb, controlId: {control.Id}, healthJobId: {healthJobId}, dqJobId:{dqJobId}");

        return dqJobId;
    }

    public async Task PurgeObserver(DHComputingJobWrapper job)
    {
        this.logger.LogInformation($"Start PurgeObserver, accountId:{job.AccountId}, controlId:{job.ControlId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");
        var dataQualityServiceClient = this.dataQualityServiceClientFactory.GetClient();
        await dataQualityServiceClient.DeleteObserver(job.TenantId, job.AccountId, job.ControlId, job.Id).ConfigureAwait(false);
        this.logger.LogInformation($"End PurgeObserver, accountId:{job.AccountId}, controlId:{job.ControlId}, healthJobId:{job.Id}, dqJobId:{job.DQJobId}");
    }
}
