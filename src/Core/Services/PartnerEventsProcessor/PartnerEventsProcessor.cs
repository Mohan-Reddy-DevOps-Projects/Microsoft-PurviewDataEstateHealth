// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using global::Azure.Messaging.EventHubs;
using global::Azure.Messaging.EventHubs.Processor;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

internal abstract class PartnerEventsProcessor : IPartnerEventsProcessor
{
    private readonly EventSourceType eventSourceType;

    private readonly AuxStorageConfiguration auxStorageConfiguration;

    private readonly IBlobStorageAccessor blobStorageAccessor;

    private readonly AzureCredentialFactory azureCredentialFactory;

    private readonly EventHubConfiguration eventHubConfiguration;

    private readonly CancellationTokenSource cancellationSource = new CancellationTokenSource();

    private EventProcessorClient EventProcessor;

    protected readonly IDataEstateHealthRequestLogger DataEstateHealthRequestLogger;

    protected readonly ICollection<ProcessEventArgs> EventsToProcess = new List<ProcessEventArgs>();

    protected PartnerEventsProcessor(
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger,
        AuxStorageConfiguration auxStorageConfiguration,
        EventHubConfiguration eventHubConfiguration,
        IBlobStorageAccessor blobStorageAccessor,
        AzureCredentialFactory azureCredentialFactory,
        EventSourceType eventSourceType)
    {
        this.eventSourceType = eventSourceType;
        this.EventsToProcess = new List<ProcessEventArgs>();
        this.DataEstateHealthRequestLogger = dataEstateHealthRequestLogger;
        this.auxStorageConfiguration = auxStorageConfiguration;
        this.eventHubConfiguration = eventHubConfiguration;
        this.blobStorageAccessor = blobStorageAccessor;
        this.azureCredentialFactory = azureCredentialFactory;
    }

    public EventSourceType EventProcessorType => this.eventSourceType;

    public async Task StartAsync(int maxProcessingTimeInSeconds = 10, int maxTimeoutInSeconds = 120)
    {
        this.DataEstateHealthRequestLogger.LogError($"Attempting to start {this.EventProcessorType} event processor.");

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
        this.DataEstateHealthRequestLogger.LogError($"Attempting to stop {this.EventProcessorType} event processor if running.");

        if (this.EventProcessor?.IsRunning == true)
        {
            await this.EventProcessor.StopProcessingAsync();
            this.EventProcessor.ProcessEventAsync -= ProcessEventHandler;
            this.EventProcessor.ProcessErrorAsync -= ProcessErrorHandler;
        }
    }

    public abstract Task CommitAsync(IDictionary<string, string> processingStoresCache = null);

    private async Task BuildEventProcessor()
    {
        var blobServiceClient = blobStorageAccessor.GetBlobServiceClient(
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
            azureCredentialFactory.CreateDefaultAzureCredential(new Uri(this.eventHubConfiguration.Authority)));

        this.EventProcessor.ProcessEventAsync += this.ProcessEventHandler;
        this.EventProcessor.ProcessErrorAsync += this.ProcessErrorHandler;
    }

    private async Task ProcessEventHandler(ProcessEventArgs eventArgs)
    {
        if (!eventArgs.HasEvent || eventArgs.Data == null || eventArgs.Partition == null)
        {
            this.DataEstateHealthRequestLogger.LogInformation($"No data read. Trying later.");
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
