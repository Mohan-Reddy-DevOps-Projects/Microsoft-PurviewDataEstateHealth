namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

public class CosmosMetricsTracker(IDataEstateHealthRequestLogger logger)
{
    public void LogCosmosMetrics<T>(string tenantId, Response<T> response)
    {
        logger.LogInformation(
            message: $"CosmosDB query metrics: TenantId = {tenantId}, RequestCharge = {response.RequestCharge}, RequestElapsedTime = {response.Diagnostics.GetClientElapsedTime()}, StatusCode = {response.StatusCode}"
        );
    }
}
