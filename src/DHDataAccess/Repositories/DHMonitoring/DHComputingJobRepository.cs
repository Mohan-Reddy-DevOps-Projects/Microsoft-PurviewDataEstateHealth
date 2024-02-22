#nullable enable
namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.JobMonitoring;
using System;

public class DHComputingJobRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonRepository<DHComputingJobWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHComputingJob";

    private string DatabaseName => configuration["cosmosDb:monitoringDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHComputingJob is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);
}
