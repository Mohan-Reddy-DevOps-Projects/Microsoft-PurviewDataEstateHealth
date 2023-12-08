// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Data.SqlClient;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;

/// <summary>
/// Synapse SQL context
/// </summary>
public sealed class SynapseSqlContext : DbContext
{
    private readonly SqlConnection sqlConnection;
    private readonly IDataEstateHealthRequestLogger logger;

    /// <summary>
    /// The database schema to use for the context.
    /// </summary>
    public readonly string databaseSchema;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynapseSqlContext"/> class.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="sqlConnection"></param>
    /// <param name="databaseSchema"></param>
    public SynapseSqlContext(DbContextOptions<SynapseSqlContext> options, IDataEstateHealthRequestLogger logger, SqlConnection sqlConnection, string databaseSchema)
        : base(options)
    {
        this.logger = logger;
        this.sqlConnection = sqlConnection;
        this.databaseSchema = databaseSchema;
    }

    /// <inheritdoc/>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // TODO: Use the method AddLoggerFactory, as the neater way of getting logs from EF. But need to implement a loggerFactory in this repo for that.
        optionsBuilder
            .UseSqlServer(sqlConnection, options => options.EnableRetryOnFailure())
            .EnableDetailedErrors()
            //.EnableSensitiveDataLogging()
            .ReplaceService<IModelCacheKeyFactory, CustomModelCacheKeyFactory>()
            .LogTo(message => this.logger.LogInformation(message), LogLevel.Information);
    }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessDomainEntity>()
            .ToTable("BusinessDomain", this.databaseSchema);
    }

    /// <summary>
    /// Gets or sets the <see cref="BusinessDomainEntity"/> entity set.
    /// </summary>
    public DbSet<BusinessDomainEntity> BusinessDomains { get; set; }
}
