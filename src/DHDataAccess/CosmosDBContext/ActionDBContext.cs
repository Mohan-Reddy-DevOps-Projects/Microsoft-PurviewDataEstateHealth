namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using global::Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.Models;

public class ActionDBContext(IConfiguration configuration) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DataHealthActionModel>(m =>
        {
            m.ToContainer("DHActionMetadata");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var cosmosDbEndpoint = this.Configuration["cosmosDb:accountEndpoint"];
        var databaseName = this.Configuration["cosmosDb:actionDatabaseName"];

        var tokenCredential = new DefaultAzureCredential();
        optionsBuilder.UseCosmos(cosmosDbEndpoint, tokenCredential, databaseName);
    }

    public DbSet<DataHealthActionModel> Actions { get; set; }

}
