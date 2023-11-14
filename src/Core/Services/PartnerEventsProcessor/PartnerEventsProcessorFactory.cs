// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

internal class PartnerEventsProcessorFactory : IPartnerEventsProcessorFactory
{
    private readonly IServiceScope scope;

    public PartnerEventsProcessorFactory(IServiceProvider serviceProvider)
    {
        this.scope = serviceProvider.CreateScope();
    }

    public IPartnerEventsProcessor Build(EventSourceType eventSourceType)
    {
        AuxStorageConfiguration auxStorageConfiguration = this.scope.ServiceProvider.GetRequiredService<IOptions<AuxStorageConfiguration>>().Value;
        IDataEstateHealthRequestLogger dataEstateHealthRequestLogger = this.scope.ServiceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
        IBlobStorageAccessor blobStorageAccessor = this.scope.ServiceProvider.GetRequiredService<IBlobStorageAccessor>();
        AzureCredentialFactory azureCredentialFactory = this.scope.ServiceProvider.GetRequiredService<AzureCredentialFactory>();

        switch (eventSourceType)
        {
            case EventSourceType.DataCatalog:
                DataCatalogEventHubConfiguration dataCatalogEventHubConfiguration = this.scope.ServiceProvider.GetRequiredService<IOptions<DataCatalogEventHubConfiguration>>().Value;
                return new DataCatalogEventsProcessor(
                    dataEstateHealthRequestLogger,
                    auxStorageConfiguration,
                    dataCatalogEventHubConfiguration,
                    blobStorageAccessor,
                    azureCredentialFactory);

            case EventSourceType.DataAccess:
                DataAccessEventHubConfiguration dataAccessEventHubConfiguration = this.scope.ServiceProvider.GetRequiredService<IOptions<DataAccessEventHubConfiguration>>().Value;
                return new DataAccessEventsProcessor(
                    dataEstateHealthRequestLogger,
                    auxStorageConfiguration,
                    dataAccessEventHubConfiguration,
                    blobStorageAccessor,
                    azureCredentialFactory);

            case EventSourceType.DataQuality:
                DataQualityEventHubConfiguration dataQualityEventHubConfiguration = this.scope.ServiceProvider.GetRequiredService<IOptions<DataQualityEventHubConfiguration>>().Value;
                return new DataQualityEventsProcessor(
                    dataEstateHealthRequestLogger,
                    auxStorageConfiguration,
                    dataQualityEventHubConfiguration,
                    blobStorageAccessor,
                    azureCredentialFactory);

            default:
                throw new ArgumentException($"Unsupported event source type: {eventSourceType}");
        }
    }
}
