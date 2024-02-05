namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using global::Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.ActiveGlossary.Scheduler.Setup.Secret;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.AttributeHandlers;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.DataHealthAction;
using System;

public class ActionDBContext(IConfiguration configuration, DHCosmosDBContextAzureCredentialManager credentialManager) : DbContext
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
        var databaseName = this.Configuration["cosmosDb:actionDatabaseName"];

        if (cosmosDbEndpoint == null)
        {
            throw new InvalidOperationException("CosmosDB accountEndpoint is not found in the configuration");
        }

        if (databaseName == null)
        {
            throw new InvalidOperationException("CosmosDB databaseName for DHAction is not found in the configuration");
        }

        var tokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ExcludeManagedIdentityCredential = false, });
        optionsBuilder.UseCosmos(cosmosDbEndpoint, credentialManager.Credential, databaseName);
    }

    public DbSet<DataHealthActionWrapper> Actions { get; set; }

}
