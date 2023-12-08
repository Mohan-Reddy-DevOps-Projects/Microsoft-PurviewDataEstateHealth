// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Identity.Client;

/// <summary>
/// OData component interface for dataset
/// </summary>
public interface IDatasetsComponent
{
    /// <summary>
    /// Gets the dataset using the OData query options
    /// </summary>
    /// <param name="query"></param>
    /// <param name="cancellationToken"></param>
    Task<IQueryable> GetDataset<T>(ODataQueryOptions<T> query, CancellationToken cancellationToken);
}

internal sealed class DatasetsComponent : IDatasetsComponent
{
    private readonly IServerlessPoolDataProvider serverlessPoolDataProvider;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly IPowerBICredentialComponent powerBICredentialComponent;
    private readonly SynapseSqlContextFactory contextFactory;
    private readonly IRequestHeaderContext requestHeaderContext;

    public DatasetsComponent(
        IServerlessPoolDataProvider serverlessPoolDataProvider,
        IDataEstateHealthRequestLogger logger,
        IPowerBICredentialComponent powerBICredentialComponent,
        SynapseSqlContextFactory contextFactory,
        IRequestHeaderContext requestHeaderContext)
    {
        this.serverlessPoolDataProvider = serverlessPoolDataProvider;
        this.logger = logger;
        this.powerBICredentialComponent = powerBICredentialComponent;
        this.contextFactory = contextFactory;
        this.requestHeaderContext = requestHeaderContext;
    }

    /// <inheritdoc/>
    public async Task<IQueryable> GetDataset<T>(ODataQueryOptions<T> query, CancellationToken cancellationToken)
    {
        Guid accountId = this.requestHeaderContext.AccountObjectId;
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(accountId, "health", cancellationToken);
        powerBICredential.Password.MakeReadOnly();
        SqlCredential sqlCredential = new(powerBICredential.UserName, powerBICredential.Password);
        IDatabaseRequest request = new DatabaseRequest
        {
            DatabaseName = "health_1",
            SchemaName = accountId.ToString()
        };

        using SynapseSqlContext dbContext = await this.contextFactory.CreateDbContextAsync(request.DatabaseName, request.SchemaName, sqlCredential, cancellationToken);

        return typeof(T) switch
        {
            Type t when t == typeof(BusinessDomainEntity) => this.serverlessPoolDataProvider.Query(query as ODataQueryOptions<BusinessDomainEntity>, dbContext.BusinessDomains),
            _ => throw new ServiceError(
                ErrorCategory.Forbidden,
                ErrorCode.NotAuthorized.Code,
                $"Dataset {typeof(T).Name} is not supported for querying").ToException(),
        };
    }
}
