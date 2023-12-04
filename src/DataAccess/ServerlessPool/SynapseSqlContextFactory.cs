// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// SynapseSqlContextFactory
/// </summary>
public class SynapseSqlContextFactory : IDbContextFactory<SynapseSqlContext>
{
    private readonly IServiceProvider provider;
    private readonly IServerlessPoolClient serverlessPoolClient;
    private readonly IDataEstateHealthLogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SynapseSqlContextFactory"/> class.
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="logger"></param>
    public SynapseSqlContextFactory(IServiceProvider provider, IDataEstateHealthLogger logger)
    {
        this.provider = provider;
        this.serverlessPoolClient = this.provider.GetService<IServerlessPoolClient>();
        this.logger = logger;
    }

    /// <inheritdoc/>
    public SynapseSqlContext CreateDbContext()
    {
        if (this.provider == null)
        {
            throw new InvalidOperationException($"You must configure an instance of IServiceProvider");
        }
        return ActivatorUtilities.CreateInstance<SynapseSqlContext>(this.provider);
    }

    /// <inheritdoc/>
    public async Task<SynapseSqlContext> CreateDbContextAsync(string database, string databaseSchema, SqlCredential sqlCredential, CancellationToken cancellationToken)
    {
        SqlConnection sqlConnection = await this.serverlessPoolClient.GetSqlConnection(sqlCredential, cancellationToken, database, useUserNamePassword: true);

        return new SynapseSqlContext(new DbContextOptions<SynapseSqlContext>(), this.logger, sqlConnection, databaseSchema);
    }
}
