namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Azure.Cosmos;
using Azure.Cosmos.Linq;
using Azure.Purview.DataEstateHealth.Loggers;
using Azure.Purview.DataEstateHealth.Models;
using CosmosDBContext;
using DHModels.Services.Control.Control;
using Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DhControlJobRepository(
    CosmosClient cosmosClient,
    IRequestHeaderContext requestHeaderContext,
    IConfiguration configuration,
    IDataEstateHealthRequestLogger logger,
    CosmosMetricsTracker cosmosMetricsTracker)
    : CommonHttpContextRepository<DhControlJobWrapper>(requestHeaderContext, logger, cosmosMetricsTracker)
{
    private const string ContainerName = "DHControlJob";
    private readonly CosmosMetricsTracker cosmosMetricsTracker = cosmosMetricsTracker;
    private readonly IDataEstateHealthRequestLogger logger = logger;
    private new PartitionKey TenantPartitionKey => new(this.AccountIdentifier.AccountId);

    private string DatabaseName =>
        configuration["cosmosDb:controlDatabaseName"]
        ?? throw new InvalidOperationException("CosmosDB databaseName for DHControlJob is not found in the configuration");

    protected override Container CosmosContainer =>
        cosmosClient.GetDatabase(this.DatabaseName)
            .GetContainer(ContainerName);

    public async Task<DhControlJobWrapper> AddAsync(DhControlJobWrapper jobWrapper, string userId)
    {
        const string methodName = nameof(AddAsync);

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, {this.AccountIdentifier.Log}"))
        {
            try
            {
                jobWrapper.OnCreate(userId);
                jobWrapper.AccountId = this.AccountIdentifier.AccountId;
                jobWrapper.TenantId = this.AccountIdentifier.TenantId;

                string jsonPayload = JsonConvert.SerializeObject(jobWrapper, Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, NullValueHandling = NullValueHandling.Ignore });

                this.logger.LogInformation($"Attempting to create job with payload:\n{jsonPayload}");

                var response = await this.CosmosContainer.CreateItemAsync(
                        jobWrapper,
                        this.TenantPartitionKey)
                    .ConfigureAwait(false);

                this.cosmosMetricsTracker.LogCosmosMetrics(this.AccountIdentifier, response);

                return response.Resource;
            }
            catch (CosmosException ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed.\n" +
                                     $"Status: {ex.StatusCode}, Sub-status: {ex.SubStatusCode}\n" +
                                     $"Entity: {JsonConvert.SerializeObject(jobWrapper, Formatting.Indented)}\n" +
                                     $"Message: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, {this.AccountIdentifier.Log}.\n" +
                                     $"Entity: {JsonConvert.SerializeObject(jobWrapper, Formatting.Indented)}", ex);
                throw;
            }
        }
    }

    public async Task<List<DhControlJobWrapper>> GetControlJobsByJobIdAsyncForAccountAndTenant(string jobId, string accountId, string tenantId)
    {
        const string methodName = nameof(GetControlJobsByJobIdAsyncForAccountAndTenant);

        var accountIdentifier = new AccountIdentifier()
        {
            AccountId = accountId,
            TenantId = tenantId
        };

        using (this.logger.LogElapsed($"{this.GetType().Name}#{methodName}, jobId = {jobId}, account = {accountIdentifier.AccountId}"))
        {
            try
            {
                this.logger.LogInformation($"Starting query with jobId: {jobId}, accountId: {accountId}, tenantId: {tenantId}");
                
                string sqlQuery = $"SELECT * FROM c WHERE c.jobId = '{jobId}'";
                this.logger.LogInformation($"Executing SQL query: {sqlQuery}");
                this.logger.LogInformation($"Using partition key: {accountIdentifier.AccountId}");

                var queryDefinition = new QueryDefinition(sqlQuery);
                var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(accountIdentifier.AccountId) };

                var rawIterator = this.CosmosContainer.GetItemQueryIterator<dynamic>(
                    queryDefinition,
                    requestOptions: queryRequestOptions);

                var results = new List<DhControlJobWrapper>();
                int pageCount = 0;

                while (rawIterator.HasMoreResults)
                {
                    pageCount++;
                    this.logger.LogInformation($"Reading page {pageCount} from Cosmos DB");
                    
                    var rawResponse = await this.retryPolicy.ExecuteAsync(
                        (context) => rawIterator.ReadNextAsync(),
                        new Context($"{this.GetType().Name}#{methodName}_{jobId}_{accountIdentifier.ConcatenatedId}")
                    ).ConfigureAwait(false);

                    this.cosmosMetricsTracker.LogCosmosMetrics(accountIdentifier, rawResponse);
                    
                    this.logger.LogInformation($"Page {pageCount} returned {rawResponse.Count} items. Request charge: {rawResponse.RequestCharge}");

                    if (rawResponse.Count <= 0)
                    {
                        continue;
                    }

                    foreach (dynamic? rawItem in rawResponse)
                    {
                        try
                        {
                            // Deserialize manually for better error handling
                            dynamic? rawJson = JsonConvert.SerializeObject(rawItem, Formatting.None);
                            dynamic? deserializedItem = JsonConvert.DeserializeObject<DhControlJobWrapper>(rawJson);

                            if (deserializedItem != null)
                            {
                                this.logger.LogInformation($"Successfully deserialized item with ID: {deserializedItem.Id}, JobId: {deserializedItem.JobId}");
                                results.Add(deserializedItem);
                            }
                            else
                            {
                                this.logger.LogWarning($"Deserialization returned null for item");
                            }
                        }
                        catch (Exception deserializationEx)
                        {
                            this.logger.LogError($"Deserialization failed for item: {deserializationEx.Message}", deserializationEx);
                        }
                    }
                }

                this.logger.LogInformation($"Query completed. Found {results.Count} control jobs with jobId: {jobId}");
                return results;
            }
            catch (Exception ex)
            {
                this.logger.LogError($"{this.GetType().Name}#{methodName} failed, jobId = {jobId}, accountId = {accountIdentifier.AccountId}, exception: {ex}", ex);
                
                // Return empty list instead of throwing
                this.logger.LogInformation($"Returning empty list due to error");
                return [];
            }
        }
    }
}