namespace DEH.Infrastructure.Repositories;

using Application.Abstractions.Catalog;
using Domain.Backfill.Catalog;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;
using Microsoft.Purview.DataGovernance.Catalog.Model;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage.Cosmos;

public abstract class BackfillCatalogRepository
{
    protected readonly Database _cosmosDatabase;
    protected const string partitionKey = "accountId";

    protected BackfillCatalogRepository(Database cosmosDatabase)
    {
        this._cosmosDatabase = cosmosDatabase;
    }
}