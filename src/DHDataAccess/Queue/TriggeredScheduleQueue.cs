// <copyright file="TriggeredScheduleQueue.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue
{
    using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.ActiveGlossary.Scheduler.Setup.Secret;

    public class TriggeredScheduleQueue(
        IOptions<TriggeredScheduleQueueConfiguration> configuration,
        DHCosmosDBContextAzureCredentialManager credentialManager,
        IDataEstateHealthRequestLogger logger) : BaseQueueService(configuration, credentialManager, logger)
    {
    }
}
