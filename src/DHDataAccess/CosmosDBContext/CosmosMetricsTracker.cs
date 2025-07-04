﻿namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories;

public class CosmosMetricsTracker(IDataEstateHealthRequestLogger logger)
{
    public void LogCosmosMetrics<T>(AccountIdentifier accountIdentifier, Response<T> response, string? queryText = null)
    {
        if (queryText != null)
        {
            logger.LogInformation(
                message: $"CosmosDB query: {accountIdentifier.Log}, Query = {queryText}",
                isSensitive: true
            );
        }
        logger.LogInformation(
            message: $"CosmosDB query metrics: {accountIdentifier.Log}, RequestCharge = {response.RequestCharge}, RequestElapsedTime = {response.Diagnostics.GetClientElapsedTime()}, StatusCode = {response.StatusCode}"
        );
    }
}
