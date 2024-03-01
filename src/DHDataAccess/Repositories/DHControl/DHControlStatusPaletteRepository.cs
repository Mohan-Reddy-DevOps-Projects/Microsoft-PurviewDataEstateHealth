namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Repositories.DHControl;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;
using System;

public class DHControlStatusPaletteRepository(CosmosClient cosmosClient, IRequestHeaderContext requestHeaderContext, IConfiguration configuration) : CommonHttpContextRepository<DHControlStatusPaletteWrapper>(requestHeaderContext)
{
    private const string ContainerName = "DHControlStatusPalette";

    private string DatabaseName => configuration["cosmosDb:controlDatabaseName"] ?? throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");

    protected override Container CosmosContainer => cosmosClient.GetDatabase(this.DatabaseName).GetContainer(ContainerName);
}
