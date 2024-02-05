namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.CosmosDBContext;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Purview.ActiveGlossary.Scheduler.Setup.Secret;
using Microsoft.Purview.DataEstateHealth.DHDataAccess.AttributeHandlers;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Control;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.MQAssessment;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Palette;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Rule.DHRuleEngine;
using Microsoft.Purview.DataEstateHealth.DHModels.Services.Score;
using System;

public class ControlDBContext(IConfiguration configuration, DHCosmosDBContextAzureCredentialManager credentialManager) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        CosmosDBAttributeHandlers.HandleCosmosDBContainerAttribute(modelBuilder);
        CosmosDBAttributeHandlers.HandleCosmosDBEnumStringAttribute(modelBuilder);
        CosmosDBAttributeHandlers.HandleCosmosDBPartitionKeyAttribute(modelBuilder);

        modelBuilder.Entity<DHRuleBaseWrapper>().HasDiscriminator(x => x.Type)
            .HasValue<DHSimpleRuleWrapper>(DHRuleBaseWrapperDerivedTypes.SimpleRule)
            .HasValue<DHExpressionRuleWrapper>(DHRuleBaseWrapperDerivedTypes.ExpressionRule)
            .HasValue<DHRuleGroupWrapper>(DHRuleBaseWrapperDerivedTypes.Group);

        modelBuilder.Entity<DHControlBaseWrapper>().HasDiscriminator(x => x.Type)
            .HasValue<DHControlGroupWrapper>(DHControlBaseWrapperDerivedTypes.Group)
            .HasValue<DHControlNodeWrapper>(DHControlBaseWrapperDerivedTypes.Node);

        modelBuilder.Entity<MQAssessmentAggregationBaseWrapper>().HasDiscriminator(x => x.Type)
            .HasValue<MQAssessmentSimpleAggregationWrapper>(MQAssessmentAggregationBaseWrapperDerivedTypes.Simple)
            .HasValue<MQAssessmentExpressionAggregationWrapper>(MQAssessmentAggregationBaseWrapperDerivedTypes.Expression);
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
        optionsBuilder.UseCosmos(cosmosDbEndpoint, credentialManager.Credential, databaseName);
    }

    public DbSet<DHControlBaseWrapper> DHControls { get; set; }

    public DbSet<DHControlStatusPaletteWrapper> DHControlStatusPalettes { get; set; }

    public DbSet<MQAssessmentWrapper> MQAssessments { get; set; }

    public DbSet<DHControlScheduleWrapper> DHControlSchedule { get; set; }

    public DbSet<DHScoreWrapper> DHScores { get; set; }

    // just let EF Core aware of these models
#pragma warning disable IDE0051 // Remove unused private members
    private DbSet<DHControlNodeWrapper> DHControlNodes { get; set; }
    private DbSet<DHControlGroupWrapper> DHControlGroups { get; set; }
#pragma warning restore IDE0051 // Remove unused private members
}
