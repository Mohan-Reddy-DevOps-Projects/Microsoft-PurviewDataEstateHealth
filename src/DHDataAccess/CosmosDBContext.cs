namespace Microsoft.Purview.DataEstateHealth.Models.v2.DataAccess;

using global::Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHCheckPoint;
using Microsoft.Purview.DataEstateHealth.DHModels.Service.Control.DHRuleEngine;
using System;

public class CosmosDBContext(IConfiguration configuration) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DHRuleBase>(m =>
        {
            m.ToContainer("DHRule");
            m.Property(e => e.Type)
                .HasConversion(
                    v => v.ToString(),
                    v => (DHRuleType)Enum.Parse(typeof(DHRuleType), v)
                );
            m.Property(e => e.CheckPoint)
                .HasConversion(
                    v => v.ToString(),
                    v => (DHCheckPoint)Enum.Parse(typeof(DHCheckPoint), v)
                );
        });

        modelBuilder.Entity<DHSimpleRule>(m =>
        {
            m.ToContainer("DHRule");
            m.Property(e => e.Operator)
                .HasConversion(
                    v => v.ToString(),
                    v => (DHOperator)Enum.Parse(typeof(DHOperator), v)
                );
        });

        modelBuilder.Entity<DHExpressionRule>(m =>
        {
            m.ToContainer("DHRule");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var cosmosDbEndpoint = this.Configuration["cosmosDb:accountEndpoint"];
        var databaseName = this.Configuration["cosmosDb:databaseName"];

        var tokenCredential = new DefaultAzureCredential();
        optionsBuilder.UseCosmos(cosmosDbEndpoint, tokenCredential, databaseName);
    }

    public DbSet<DHRuleBase> Rules { get; set; }
}
