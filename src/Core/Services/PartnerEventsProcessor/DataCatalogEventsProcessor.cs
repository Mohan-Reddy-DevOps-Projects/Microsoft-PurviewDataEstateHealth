// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

internal class DataCatalogEventsProcessor : PartnerEventsProcessor
{
    public DataCatalogEventsProcessor(
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger,
        AuxStorageConfiguration auxStorageConfiguration,
        DataCatalogEventHubConfiguration eventHubConfiguration,
        IBlobStorageAccessor blobStorageAccessor,
        AzureCredentialFactory azureCredentialFactory)
        : base(
            dataEstateHealthRequestLogger,
            auxStorageConfiguration,
            eventHubConfiguration, blobStorageAccessor,
            azureCredentialFactory,
            EventSourceType.DataCatalog)
    {
    }

    public override async Task CommitAsync(IDictionary<string, string> processingStoresCache = null)
    {
        await Task.CompletedTask;
        this.DataEstateHealthRequestLogger.LogInformation($"Attempting to commit {this.EventsToProcess.Count} rows of {this.EventProcessorType}.");
    }
}
