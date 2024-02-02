namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using global::Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.AttributeHandlers;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using System;

public class ControlDBContext(IConfiguration configuration) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CosmosDBAttributeHandlers.HandleCosmosDBContainerAttribute(modelBuilder);
        CosmosDBAttributeHandlers.HandleCosmosDBEnumStringAttribute(modelBuilder);
        CosmosDBAttributeHandlers.HandleCosmosDBPartitionKeyAttribute(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var cosmosDbEndpoint = this.Configuration["cosmosDb:accountEndpoint"];
        var databaseName = this.Configuration["cosmosDb:controlDatabaseName"];

        if (cosmosDbEndpoint == null)
        {
            throw new InvalidOperationException("CosmosDB accountEndpoint is not found in the configuration");
        }

        if (databaseName == null)
        {
            throw new InvalidOperationException("CosmosDB databaseName for DHControl is not found in the configuration");
        }

        var tokenCredential = new DefaultAzureCredential();
        optionsBuilder.UseCosmos(cosmosDbEndpoint, tokenCredential, databaseName);
    }

    public DbSet<DHControlNodeWrapper> DHControlNodes { get; set; }

    public DbSet<DHControlGroupWrapper> DHControlGroups { get; set; }
}
