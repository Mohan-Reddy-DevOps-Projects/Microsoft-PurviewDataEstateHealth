namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using System;

public class DHControlScheduleRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonRepository<DHControlScheduleWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHSchedule";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);
}