namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using global::Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.AttributeHandlers;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;

public class ControlDBContext(IConfiguration configuration) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CosmosDBAttributeHandlers.HandleCosmosDBContainerAttribute(modelBuilder);
        CosmosDBAttributeHandlers.HandleCosmosDBEnumStringAttribute(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var cosmosDbEndpoint = this.Configuration["cosmosDb:accountEndpoint"];
        var databaseName = this.Configuration["cosmosDb:controlDatabaseName"];

        var tokenCredential = new DefaultAzureCredential();
        optionsBuilder.UseCosmos(cosmosDbEndpoint, tokenCredential, databaseName);
    }

    public DbSet<DHRuleOrGroupBase> DHRuleOrGroups { get; set; }

    public DbSet<DHRuleBase> DHRules { get; set; }

    public DbSet<DHSimpleRule> DHSimpleRules { get; set; }

    public DbSet<DHExpressionRule> DHExpressionRules { get; set; }

    public DbSet<DHRuleGroup> DHRuleGroups { get; set; }

    public DbSet<DHControlBase> DHControls { get; set; }

    public DbSet<DHControlNode> DHControlNodes { get; set; }

    public DbSet<DHControlGroup> DHControlGroups { get; set; }
}
