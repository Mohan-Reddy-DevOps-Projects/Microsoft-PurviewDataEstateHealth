// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using global::Azure.Messaging.EventHubs;
using global::Azure.Messaging.EventHubs.Processor;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataEstateHealth.Core;
using Microsoft.Purview.DataGovernance.Common;
using Microsoft.Purview.DataGovernance.DeltaWriter;
using Microsoft.Purview.DataGovernance.Reporting.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

internal abstract class PartnerEventsProcessor : IPartnerEventsProcessor
{
    private readonly AuxStorageConfiguration auxStorageConfiguration;

    private readonly IBlobStorageAccessor blobStorageAccessor;

    private readonly IProcessingStorageManager processingStorageManager;

    private readonly EventHubConfiguration eventHubConfiguration;

    private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();

    private EventProcessorClient EventProcessor;

    private readonly IDeltaTableEventProcessor deltaTableEventProcessor;

    protected readonly IDataEstateHealthRequestLogger DataEstateHealthRequestLogger;

    protected readonly AzureCredentialFactory AzureCredentialFactory;

    protected readonly IDeltaLakeOperatorFactory DeltaWriterFactory;

    protected readonly IDictionary<Guid, IList<ProcessEventArgs>> EventArgsToCheckpoint;

    protected readonly ICollection<ProcessEventArgs> EventsToProcess;

    protected IDictionary<Guid, string> ProcessingStorageCache;

    protected PartnerEventsProcessor(
        IServiceProvider serviceProvider,
        EventHubConfiguration eventHubConfiguration,
        EventSourceType eventSourceType)
    {
        this.EventProcessorType = eventSourceType;
        this.EventsToProcess = new List<ProcessEventArgs>();
        this.deltaTableEventProcessor = serviceProvider.GetRequiredService<IDeltaTableEventProcessor>();
        this.DataEstateHealthRequestLogger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
        this.auxStorageConfiguration = serviceProvider.GetRequiredService<IOptions<AuxStorageConfiguration>>().Value;
        this.eventHubConfiguration = eventHubConfiguration;
        this.blobStorageAccessor = serviceProvider.GetRequiredService<IBlobStorageAccessor>();
        this.AzureCredentialFactory = serviceProvider.GetRequiredService<AzureCredentialFactory>();
        this.processingStorageManager = serviceProvider.GetRequiredService<IProcessingStorageManager>();
        this.DeltaWriterFactory = serviceProvider.GetRequiredService<IDeltaLakeOperatorFactory>();

        this.EventsToProcess = new List<ProcessEventArgs>();
        this.ProcessingStorageCache = new Dictionary<Guid, string>();
        this.EventArgsToCheckpoint = new Dictionary<Guid, IList<ProcessEventArgs>>();
    }

    public EventSourceType EventProcessorType { get; }

    public async Task StartAsync(int maxProcessingTimeInSeconds = 10, int maxTimeoutInSeconds = 120)
    {
        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to start {this.EventProcessorType} event processor.");

        try
        {
            this.cancellationSource.CancelAfter(TimeSpan.FromSeconds(maxTimeoutInSeconds));

            await this.BuildEventProcessor();

            await this.EventProcessor.StartProcessingAsync();

            await Task.Delay(TimeSpan.FromSeconds(maxProcessingTimeInSeconds), this.cancellationSource.Token);
        }
        catch (TaskCanceledException exception)
        {
            // This is expected when the operation is canceled.
            this.DataEstateHealthRequestLogger.LogWarning($"Event processing cancelled in {this.EventProcessorType} event processor.", exception);
        }
    }

    public async Task StopAsync()
    {
        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to stop {this.EventProcessorType} event processor if running.");

        if (this.EventProcessor?.IsRunning == true)
        {
            await this.EventProcessor.StopProcessingAsync();
            this.EventProcessor.ProcessEventAsync -= this.ProcessEventHandler;
            this.EventProcessor.ProcessErrorAsync -= this.ProcessErrorHandler;
        }
    }

    public abstract Task CommitAsync(IDictionary<Guid, string> processingStoresCache = null);

    protected void ParseEventPayload<T>(EventHubModel eventHubModel, Dictionary<EventOperationType, List<T>> entityModels)
        where T : BaseEventHubEntityModel
    {
        try
        {
            T entityModel = JsonConvert.DeserializeObject<T>((eventHubModel.AlternatePayload ?? eventHubModel.Payload.After ?? eventHubModel.Payload.Before).ToString());
            entityModel.AccountId = eventHubModel.AccountId.ToString();
            entityModel.EventId = eventHubModel.EventId.ToString();
            entityModel.EventCorrelationId = eventHubModel.EventCorrelationId.ToString();
            entityModel.EventCreationTimestamp = eventHubModel.EventCreationTimestamp;

            if (!entityModels.ContainsKey(eventHubModel.OperationType))
            {
                entityModels.Add(eventHubModel.OperationType, new List<T> { entityModel });
            }
            else
            {
                entityModels[eventHubModel.OperationType].Add(entityModel);
            }
        }
        catch (JsonException exception)
        {
            this.DataEstateHealthRequestLogger.LogError($"Failed to parse event payload of type: {eventHubModel.PayloadKind}.", exception);
        }
    }

    /// <summary>
    /// Persists data to storage based on event operation types.
    /// </summary>
    /// <param name="eventHubEntityModels">A dictionary of event hub entity models.</param>
    /// <param name="deltaTableWriter">The Delta Lake table writer.</param>
    /// <param name="prefix">Prefix for the path.</param>
    /// <param name="isSourceEvent">Flag to determine source event.</param>
    protected async Task PersistToStorage<T>(Dictionary<EventOperationType, List<T>> eventHubEntityModels, IDeltaLakeOperator deltaTableWriter, string prefix, bool isSourceEvent = true)
        where T : BaseEventHubEntityModel
    {
        await this.deltaTableEventProcessor.PersistToStorage(eventHubEntityModels, deltaTableWriter, prefix, isSourceEvent);
    }

    protected async Task CommitCheckpoints()
    {
        if (this.EventArgsToCheckpoint.Count < 1)
        {
            return;
        }

        try
        {
            foreach (var eventArgs in this.EventArgsToCheckpoint)
            {
                await Task.WhenAll(eventArgs.Value.Select(e => e.UpdateCheckpointAsync()).ToArray());
            }

            this.DataEstateHealthRequestLogger.LogInformation($"Committed event checkpoint(s) for {this.EventArgsToCheckpoint.Count} accounts(s).");
        }
        catch (Exception exception)
        {
            // Ideally should not reach here
            this.DataEstateHealthRequestLogger.LogCritical($"Failed to commit {this.EventProcessorType} event checkpoint(s).", exception);
        }
    }

    protected async Task<bool> ProcessingStorageExists(Guid accountId)
    {
        if (!this.ProcessingStorageCache.ContainsKey(accountId))
        {
            ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountId, CancellationToken.None);

            if (storageModel == null)
            {
                this.DataEstateHealthRequestLogger.LogError($"Un-provisioned Purview account {accountId} encountered.");
                this.EventArgsToCheckpoint.Remove(accountId);
                return false;
            }

            this.ProcessingStorageCache.Add(accountId, $"{storageModel.GetDfsEndpoint()}/{storageModel.CatalogId}");
        }

        return true;
    }

    protected Dictionary<Guid, List<T>> GetEventsByAccount<T>()
        where T : BaseEventHubModel
    {
        Dictionary<Guid, List<T>> eventsByAccount = new();
        foreach (ProcessEventArgs eventArgs in this.EventsToProcess)
        {
            if (eventArgs.Data == null || eventArgs.Partition == null)
            {
                continue;
            }

            this.DataEstateHealthRequestLogger.LogTrace($"Current position Event Props - Partition: {eventArgs.Partition.PartitionId} | Sequence: {eventArgs.Data.SequenceNumber} | Offset: {eventArgs.Data.Offset}.");
            var eventMessage = Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray());

            try
            {
                T eventModel = JsonConvert.DeserializeObject<T>(eventMessage);
                Guid accountId = eventModel.AccountId;

                List<T> eventList;
                if (eventsByAccount.TryGetValue(accountId, out eventList))
                {
                    eventList.Add(eventModel);
                    this.EventArgsToCheckpoint[accountId].Add(eventArgs);
                }
                else
                {
                    eventsByAccount.Add(accountId, new List<T> { eventModel });
                    this.EventArgsToCheckpoint.Add(accountId, new List<ProcessEventArgs>() { eventArgs });
                }
            }
            catch (JsonException exception)
            {
                this.DataEstateHealthRequestLogger.LogError($"Failed to parse event payload: {eventMessage}.", exception);
                continue;
            }
        }

        return eventsByAccount;
    }

    private async Task BuildEventProcessor()
    {
        var blobServiceClient = this.blobStorageAccessor.GetBlobServiceClient(
                this.auxStorageConfiguration.AccountName,
                this.auxStorageConfiguration.EndpointSuffix,
                this.auxStorageConfiguration.BlobStorageResource);

        var storageClient = blobServiceClient.GetBlobContainerClient(this.eventHubConfiguration.EventCheckpointsContainerName);
        await storageClient.CreateIfNotExistsAsync();

        this.EventProcessor = new EventProcessorClient(
            storageClient,
            this.eventHubConfiguration.ConsumerGroup,
            this.eventHubConfiguration.EventHubNamespace,
            this.eventHubConfiguration.EventHubName,
            this.AzureCredentialFactory.CreateDefaultAzureCredential(new Uri(this.eventHubConfiguration.Authority)));

        this.EventProcessor.ProcessEventAsync += this.ProcessEventHandler;
        this.EventProcessor.ProcessErrorAsync += this.ProcessErrorHandler;
    }

    private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        if (!eventArgs.HasEvent || eventArgs.Data == null || eventArgs.Partition == null)
        {
            this.DataEstateHealthRequestLogger.LogWarning($"No data read. Trying later.");
            return;
        }

        if (this.EventsToProcess.Count >= this.eventHubConfiguration.MaxEventsToProcess)
        {
            this.cancellationSource.Cancel(true);
            return;
        }

        this.EventsToProcess.Add(eventArgs);
        await Task.CompletedTask;
    }

    private async Task ProcessErrorHandler(ProcessErrorEventArgs eventArgs)
    {
        this.DataEstateHealthRequestLogger.LogError($"Encountered error while processing event - Partition: {eventArgs.PartitionId} | Operation: {eventArgs.Operation}.", eventArgs.Exception);
        await Task.CompletedTask;
    }
}
