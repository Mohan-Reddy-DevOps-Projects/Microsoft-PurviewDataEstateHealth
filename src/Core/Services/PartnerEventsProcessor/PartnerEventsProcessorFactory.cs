﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

internal class PartnerEventsProcessorFactory : IPartnerEventsProcessorFactory
{
    private readonly IServiceScope scope;
    private readonly IServiceProvider serviceProvider;

    public PartnerEventsProcessorFactory(IServiceProvider serviceProvider)
    {
        this.scope = serviceProvider.CreateScope();
        this.serviceProvider = serviceProvider;
    }

    public IPartnerEventsProcessor Build(EventSourceType eventSourceType)
    {
        switch (eventSourceType)
        {
            case EventSourceType.DataCatalog:
                return new DataCatalogEventsProcessor(
                    this.scope.ServiceProvider,
                    this.scope.ServiceProvider.GetRequiredService<IOptions<DataCatalogEventHubConfiguration>>().Value);

            case EventSourceType.DataAccess:
                return new DataAccessEventsProcessor(
                    this.scope.ServiceProvider,
                    this.scope.ServiceProvider.GetRequiredService<IOptions<DataAccessEventHubConfiguration>>().Value);

            case EventSourceType.DataQuality:
                return new DataQualityEventsProcessor(
                    this.scope.ServiceProvider,
                    this.scope.ServiceProvider.GetRequiredService<IOptions<DataQualityEventHubConfiguration>>().Value,
                    this.serviceProvider.GetService<IDataHealthApiService>());

            default:
                throw new ArgumentException($"Unsupported event source type: {eventSourceType}");
        }
    }
}
