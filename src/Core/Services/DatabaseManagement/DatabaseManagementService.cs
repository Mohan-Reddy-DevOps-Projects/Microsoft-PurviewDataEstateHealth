// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class DatabaseManagementService : IDatabaseManagementService
{
    private const string DatabaseName = "health_1";

    private readonly IDatabaseCommand databaseCommand;

    private readonly IPowerBICredentialComponent powerBICredentialComponent;
    private readonly IProcessingStorageManager processingStorageManager;
    private readonly IDataEstateHealthRequestLogger dataEstateHealthRequestLogger;
    private IDatabaseRequest setupSQLRequest;

    public DatabaseManagementService(IDatabaseCommand databaseCommand, IPowerBICredentialComponent powerBICredentialComponent, IProcessingStorageManager processingStorageManager, IDataEstateHealthRequestLogger logger)
    {
        this.databaseCommand = databaseCommand;
        this.powerBICredentialComponent = powerBICredentialComponent;
        this.processingStorageManager = processingStorageManager;
        this.dataEstateHealthRequestLogger = logger;
    }

    public async Task Initialize(AccountServiceModel accountModel, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to initialize database related resources"))
        {
            try
            {
                IDatabaseRequest databaseRequest = new DatabaseRequest
                {
                    DatabaseName = DatabaseName,
                };
                await this.databaseCommand.AddDatabaseAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Database created successfully");

                Guid accountId = Guid.Parse(accountModel.Id);
                PowerBICredential powerBICredential = await this.powerBICredentialComponent.GetSynapseDatabaseLoginInfo(accountId, OwnerNames.Health, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("PowerBI credential retrieved successfully");
                if (powerBICredential == null)
                {
                    // If the credential doesn't exist, lets create one. Otherwise this logic can be skipped
                    powerBICredential = this.powerBICredentialComponent.CreateCredential(accountId, OwnerNames.Health);
                    await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseLoginInfo(powerBICredential, cancellationToken);
                    this.dataEstateHealthRequestLogger.LogInformation("PowerBI credential created successfully");
                }

                DatabaseMasterKey databaseMasterKey = await this.powerBICredentialComponent.GetSynapseDatabaseMasterKey(DatabaseName, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Master key retrieved successfully");
                if (databaseMasterKey == null)
                {
                    databaseMasterKey = this.powerBICredentialComponent.CreateMasterKey(DatabaseName);
                    await this.powerBICredentialComponent.AddOrUpdateSynapseDatabaseMasterKey(databaseMasterKey, cancellationToken);
                    this.dataEstateHealthRequestLogger.LogInformation("Master key created successfully");
                }

                Models.ProcessingStorageModel storageModel = await this.processingStorageManager.Get(accountModel, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Storage model retrieved successfully");
                ArgumentNullException.ThrowIfNull(storageModel, nameof(storageModel));

                databaseRequest = new DatabaseRequest
                {
                    DatabaseName = DatabaseName,
                    DataSourceLocation = $"{storageModel.GetDfsEndpoint()}/{accountModel.DefaultCatalogId}/",
                    SchemaName = accountId.ToString(),
                    LoginName = powerBICredential.LoginName,
                    LoginPassword = powerBICredential.Password,
                    UserName = powerBICredential.UserName,
                    MasterKey = databaseMasterKey.MasterKey,
                    ScopedCredential = new ManagedIdentityScopedCredential("SynapseMICredential")
                };

                this.setupSQLRequest = databaseRequest;

                await this.databaseCommand.AddMasterKeyAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Master key added successfully");

                await this.databaseCommand.AddScopedCredentialAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Scoped credential added successfully");

                await this.databaseCommand.AddLoginAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Login added successfully");

                await this.databaseCommand.AddUserAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("User added successfully");

                await this.databaseCommand.CreateSchemaAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Schema created successfully");

                await this.databaseCommand.GrantUserToSchemaAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("User granted to schema successfully");

                await this.databaseCommand.GrantCredentialToUserAsync(databaseRequest, cancellationToken);
                this.dataEstateHealthRequestLogger.LogInformation("Credential granted to user successfully");
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError("Failed to initialize database related resources", e);
                throw;
            }
        }
    }

    public async Task RunSetupSQL(CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to run setup SQL"))
        {
            if (this.setupSQLRequest == null)
            {
                this.dataEstateHealthRequestLogger.LogCritical("SQL should be runed after initialize");
                throw new Exception("Failed to run setup SQL as request context is empty");
            }
            try
            {
                await this.databaseCommand.ExecuteSetupScriptAsync(this.setupSQLRequest, cancellationToken);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError("Failed to run setup SQL", e);
                throw;
            }
        }
    }

    public async Task Deprovision(AccountServiceModel accountModel, CancellationToken cancellationToken)
    {
        using (this.dataEstateHealthRequestLogger.LogElapsed("start to delete database related resources"))
        {
            var accountId = Guid.Parse(accountModel.Id);

            var databaseRequest = new DatabaseRequest
            {
                DatabaseName = DatabaseName,
                SchemaName = accountId.ToString(),
                ScopedCredential = new ManagedIdentityScopedCredential("SynapseMICredential")
            };
            try
            {
                await this.databaseCommand.ExecuteSetupRollbackScriptAsync(databaseRequest, cancellationToken);
            }
            catch (Exception e)
            {
                this.dataEstateHealthRequestLogger.LogError("Failed to rollback database related resources", e);
                throw;
            }
        }
    }
}
