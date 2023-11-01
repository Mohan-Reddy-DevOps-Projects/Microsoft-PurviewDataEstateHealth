// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class DatabaseManagementService : IDatabaseManagementService
{
    private const string DatabaseName = "health_1";

    private readonly IDatabaseCommand databaseCommand;

    private readonly IPowerBICredentialComponent powerBICredentialComponent;

    public DatabaseManagementService(IDatabaseCommand databaseCommand, IPowerBICredentialComponent powerBICredentialComponent)
    {
        this.databaseCommand = databaseCommand;
        this.powerBICredentialComponent = powerBICredentialComponent;
    }

    public async Task Initialize(Guid accountId, CancellationToken cancellationToken)
    {
        IDatabaseRequest databaseRequest = new DatabaseRequest
        {
            DatabaseName = DatabaseName,
        };
        await this.databaseCommand.AddDatabaseAsync(databaseRequest, cancellationToken);

        string owner = "health";
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

        databaseRequest = new DatabaseRequest
        {
            DatabaseName = DatabaseName,
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
