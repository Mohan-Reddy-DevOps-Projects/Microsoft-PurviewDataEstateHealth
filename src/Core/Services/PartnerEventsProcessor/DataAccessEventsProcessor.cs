﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.DeltaWriter;

internal class DataAccessEventsProcessor : PartnerEventsProcessor
{
    private readonly DataAccessEventHubConfiguration eventHubConfiguration;

    public DataAccessEventsProcessor(
        IServiceProvider serviceProvider,
        DataAccessEventHubConfiguration eventHubConfiguration)
        : base(
            serviceProvider,
            eventHubConfiguration,
            EventSourceType.DataAccess)
    {
        this.eventHubConfiguration = eventHubConfiguration;
    }

    public override async Task CommitAsync(IDictionary<Guid, string> processingStorageCache = null)
    {
        if (processingStorageCache != null)
        {
            this.ProcessingStorageCache = processingStorageCache;
        }

        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to commit {this.EventsToProcess.Count} rows of {this.EventProcessorType}.");

        Dictionary<Guid, List<EventHubModel>> eventsByAccount = this.GetEventsByAccount<EventHubModel>();

        foreach (var accountEvents in eventsByAccount)
        {
            try
            {
                await this.UploadEventsForAccount(accountEvents.Key, accountEvents.Value);
            }
            catch (Exception exception)
            {
                this.DataEstateHealthRequestLogger.LogCritical($"Failed to upload {accountEvents.Value.Count} events of {this.EventProcessorType} for account: {accountEvents.Key}.", exception);
                this.EventArgsToCheckpoint.Remove(accountEvents.Key);
            }
        }

        await this.CommitCheckpoints();
    }

    private async Task UploadEventsForAccount(Guid accountId, List<EventHubModel> events)
    {
        if (await this.ProcessingStorageExists(accountId) != true)
        {
            return;
        }

        string storageEndpoint = this.ProcessingStorageCache[accountId];
        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to persisting events under {storageEndpoint} for account Id: {accountId}.");

        Dictionary<EventOperationType, List<PolicySetEventHubEntityModel>> policySetModels = new();
        Dictionary<EventOperationType, List<DataSubscriptionEventHubEntityModel>> dataSubscriptionModels = new();

        foreach (EventHubModel eventHubModel in events)
        {
            switch (eventHubModel.PayloadKind)
            {
                case PayloadKind.PolicySet:
                    this.ParseEventPayload(eventHubModel, policySetModels);
                    break;
                case PayloadKind.DataSubscription:
                    this.ParseEventPayload(eventHubModel, dataSubscriptionModels);
                    break;
                default:
                    this.DataEstateHealthRequestLogger.LogWarning($"Encountered unsupported data access event kind: {eventHubModel.PayloadKind}.");
                    break;
            }
        }

        IDeltaLakeOperator deltaTableWriter = this.DeltaWriterFactory.Build(new Uri(storageEndpoint),
            this.AzureCredentialFactory.CreateDefaultAzureCredential(new Uri(this.eventHubConfiguration.Authority)));

        await this.PersistToStorage(policySetModels, deltaTableWriter, nameof(EventSourceType.DataAccess));
        await this.PersistToStorage(dataSubscriptionModels, deltaTableWriter, nameof(EventSourceType.DataAccess));
        this.DataEstateHealthRequestLogger.LogInformation($"Persisted {events.Count} {EventSourceType.DataAccess} events for account Id: {accountId}.");
    }
}
