// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using global::Azure.Core;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class DatabaseManagementService : IDatabaseManagementService
{
    private const string DatabaseName = "health_1";

    private readonly IDatabaseCommand databaseCommand;

    private readonly IPowerBICredentialComponent powerBICredentialComponent;
    private readonly IProcessingStorageManager processingStorageManager;

    public DatabaseManagementService(IDatabaseCommand databaseCommand, IPowerBICredentialComponent powerBICredentialComponent, IProcessingStorageManager processingStorageManager)
    {
        this.databaseCommand = databaseCommand;
        this.powerBICredentialComponent = powerBICredentialComponent;
        this.processingStorageManager = processingStorageManager;
    }

    public async Task Initialize(AccountServiceModel accountModel, CancellationToken cancellationToken)
    {
        IDatabaseRequest databaseRequest = new DatabaseRequest
        {
            DatabaseName = DatabaseName,
        };
        await this.databaseCommand.AddDatabaseAsync(databaseRequest, cancellationToken);

        string owner = "health";
        Guid accountId = Guid.Parse(accountModel.Id);
        PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(accountId, owner, cancellationToken);
        if (powerBICredential == null)
        {
            // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
            powerBICredential = this.powerBICredentialComponent.CreateCredential(accountId, owner);
            await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
        }

        DatabaseMasterKey databaseMasterKey = await this.powerBICredentialComponent.GetSynapseDatabaseMasterKey(DatabaseName, cancellationToken);
        if (databaseMasterKey == null)
        {
            databaseMasterKey = this.powerBICredentialComponent.CreateMasterKey(DatabaseName);
            await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseMasterKey(databaseMasterKey, cancellationToken);
        }

        Models.ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountModel, cancellationToken);
        ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));
        string storageAccountName = storageModel.GetStorageAccountName();

        databaseRequest = new DatabaseRequest
        {
            DatabaseName = DatabaseName,
            DataSourceLocation = $"https://{storageAccountName}.{storageModel.Properties.DnsZone}.dfs.{storageModel.Properties.EndpointSuffix}/{accountModel.DefaultCatalogId}/",
            SchemaName = accountId.ToString(),
            LoginName = powerBICredential.LoginName,
            LoginPassword = powerBICredential.Password,
            UserName = powerBICredential.UserName,
            MasterKey = databaseMasterKey.MasterKey,
            ScopedCredential = new ManagedIdentityScopedCredential("SynapseMICredential")
        };

        await this.databaseCommand.AddMasterKeyAsync(databaseRequest, cancellationToken);
        await this.databaseCommand.AddScopedCredentialAsync(databaseRequest, cancellationToken);
        await this.databaseCommand.AddLoginAsync(databaseRequest, cancellationToken);
        await this.databaseCommand.AddUserAsync(databaseRequest, cancellationToken);
        await this.databaseCommand.CreateSchemaAsync(databaseRequest, cancellationToken);
        await this.databaseCommand.GrantUserToSchemaAsync(databaseRequest, cancellationToken);
        await this.databaseCommand.GrantCredentialToUserAsync(databaseRequest, cancellationToken);

        await this.databaseCommand.ExecuteScriptAsync(databaseRequest, cancellationToken);
    }
}
