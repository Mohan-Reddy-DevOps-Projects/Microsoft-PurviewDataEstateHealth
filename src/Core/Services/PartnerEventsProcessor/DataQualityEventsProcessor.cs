// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

internal class DataQualityEventsProcessor : PartnerEventsProcessor
{
    public DataQualityEventsProcessor(
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger,
        AuxStorageConfiguration auxStorageConfiguration,
        DataQualityEventHubConfiguration eventHubConfiguration,
        IBlobStorageAccessor blobStorageAccessor,
        AzureCredentialFactory azureCredentialFactory)
        : base(
            dataEstateHealthRequestLogger,
            auxStorageConfiguration,
            eventHubConfiguration, blobStorageAccessor,
            azureCredentialFactory,
            EventSourceType.DataAccess)
    {
    }

    public override async Task CommitAsync(IDictionary<string, string> processingStoresCache = null)
    {
        await Task.CompletedTask;
        this.DataEstateHealthRequestLogger.LogInformation($"Attempting to commit {this.EventsToProcess.Count} rows of {this.EventProcessorType}.");
    }
}
