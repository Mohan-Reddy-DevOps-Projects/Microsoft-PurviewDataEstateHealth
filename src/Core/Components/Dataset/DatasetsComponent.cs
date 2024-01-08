// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using Microsoft.Purview.DataGovernance.DataLakeAPI.Entities;
using Microsoft.Purview.DataGovernance.Reporting.Models;

/// <summary>
/// OData component interface for dataset
/// </summary>
public interface IDatasetsComponent
{
    /// <summary>
    /// Gets the dataset using the OData query options
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="query"></param>
    /// <returns></returns>
    IQueryable GetDataset<T>(Func<IQueryable<T>> query) where T : class;

    /// <summary>
    /// Gets the context for the dataset
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<SynapseSqlContext> GetContext(CancellationToken cancellationToken);
}

internal sealed class DatasetsComponent : IDatasetsComponent
{
    private readonly IDatasetsProvider datasetsProvider;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IPowerBICredentialComponent powerBICredentialComponent;
    private readonly IRequestHeaderContext requestHeaderContext;

    public DatasetsComponent(
        IDatasetsProvider datasetsProvider,
        IDataEstateHealthRequestLogger logger,
        IPowerBICredentialComponent powerBICredentialComponent,
        IRequestHeaderContext requestHeaderContext)
    {
        this.datasetsProvider = datasetsProvider;
        this.logger = logger;
        this.powerBICredentialComponent = powerBICredentialComponent;
        this.requestHeaderContext = requestHeaderContext;
    }
    public IQueryable GetDataset<T>(Func<IQueryable<T>> query) where T : class
    {
        return this.datasetsProvider.GetDataset(query);
    }

    public async Task<SynapseSqlContext> GetContext(CancellationToken cancellationToken)
    {
        Guid accountId = this.requestHeaderContext.AccountObjectId;
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(accountId, "health", cancellationToken);
        powerBICredential.Password.MakeReadOnly();
        SqlCredential sqlCredential = new(powerBICredential.UserName, powerBICredential.Password);

        return await this.datasetsProvider.GetContext(new DataLakeQueryContext()
        {
            DatabaseName = "health_1",
            SchemaName = accountId.ToString(),
            SqlCredenial = sqlCredential,
            EntityConfigurations = new List<IEntityConfiguration>()
            {
                new EntityConfiguration()
            }
        }, cancellationToken);
    }
}

internal class EntityConfiguration : IEntityConfiguration
{
    /// <inheritdoc/>
    public void Configure(ModelBuilder modelBuilder, string databaseSchema)
    {
        modelBuilder.Entity<BusinessDomainEntity>()
            .ToTable("BusinessDomain", databaseSchema);
    }
}
