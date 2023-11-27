// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using global::Azure.Messaging.EventHubs.Processor;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Newtonsoft.Json;

internal class DataCatalogEventsProcessor : PartnerEventsProcessor
{
    private readonly DataCatalogEventHubConfiguration eventHubConfiguration;

    public DataCatalogEventsProcessor(
        IServiceProvider serviceProvider,
        DataCatalogEventHubConfiguration eventHubConfiguration
        )
        : base(
            serviceProvider,
            eventHubConfiguration,
            EventSourceType.DataCatalog)
    {
        this.eventHubConfiguration = eventHubConfiguration;
    }

    public override async Task CommitAsync(IDictionary<string, string> processingStorageCache = null)
    {
        if (processingStorageCache != null)
        {
            this.ProcessingStorageCache = processingStorageCache;
        }

        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to commit {this.EventsToProcess.Count} rows of {this.EventProcessorType}.");

        Dictionary<string, List<EventHubModel>> eventsByAccount = this.GetEventsByAccount<EventHubModel>();

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

    private async Task UploadEventsForAccount(string accountId, List<EventHubModel> events)
    {
        if (await this.ProcessingStorageExists(accountId) != true)
        {
            return;
        }

        string storageEndpoint = this.ProcessingStorageCache[accountId];
        this.DataEstateHealthRequestLogger.LogTrace($"Attempting to persisting events under {storageEndpoint} for account Id: {accountId}.");

        Dictionary<EventOperationType, List<BusinessDomainEventHubEntityModel>> businessDomainModels = new();
        Dictionary<EventOperationType, List<DataProductEventHubEntityModel>> dataProductModels = new();
        Dictionary<EventOperationType, List<DataAssetEventHubEntityModel>> dataAssetModels = new();
        Dictionary<EventOperationType, List<RelationshipEventHubEntityModel>> relationshipModels = new();
        Dictionary<EventOperationType, List<TermEventHubEntityModel>> termModels = new();

        foreach (EventHubModel eventHubModel in events)
        {
            switch (eventHubModel.PayloadKind)
            {
                case PayloadKind.BusinessDomain:
                    this.ParseEventPayload(eventHubModel, businessDomainModels);
                    break;
                case PayloadKind.DataProduct:
                    this.ParseEventPayload(eventHubModel, dataProductModels);
                    break;
                case PayloadKind.DataAsset:
                    this.ParseEventPayload(eventHubModel, dataAssetModels);
                    break;
                case PayloadKind.Relationship:
                    this.ParseEventPayload(eventHubModel, relationshipModels);
                    break;
                case PayloadKind.Term:
                    this.ParseEventPayload(eventHubModel, termModels);
                    break;
                default:
                    this.DataEstateHealthRequestLogger.LogWarning($"Encountered unsupported catalog event kind: {eventHubModel.PayloadKind}.");
                    break;
            }
        }

        IDeltaLakeOperator deltaTableWriter = this.DeltaWriterFactory.Build(new Uri(storageEndpoint),
            this.AzureCredentialFactory.CreateDefaultAzureCredential(new Uri(this.eventHubConfiguration.Authority)));

        await this.PersistToStorage(businessDomainModels, deltaTableWriter, nameof(EventSourceType.DataCatalog));
        await this.PersistToStorage(dataProductModels, deltaTableWriter, nameof(EventSourceType.DataCatalog));
        await this.PersistToStorage(dataAssetModels, deltaTableWriter, nameof(EventSourceType.DataCatalog));
        await this.PersistToStorage(relationshipModels, deltaTableWriter, nameof(EventSourceType.DataCatalog));
        await this.PersistToStorage(termModels, deltaTableWriter, nameof(EventSourceType.DataCatalog));
        this.DataEstateHealthRequestLogger.LogInformation($"Persisted {events.Count} {EventSourceType.DataCatalog} events for account Id: {accountId}.");
    }
}
