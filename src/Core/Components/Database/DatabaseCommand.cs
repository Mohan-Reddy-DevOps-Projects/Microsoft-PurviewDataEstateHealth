// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Data.SqlClient;
using Microsoft.Purview.DataGovernance.DataLakeAPI;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

internal class DatabaseCommand : IDatabaseCommand
{
    private readonly IServerlessPoolClient serverlessPoolClient;

    private readonly IDataEstateHealthRequestLogger logger;

    public DatabaseCommand(IServerlessPoolClient serverlessPoolClient, IDataEstateHealthRequestLogger logger)
    {
        this.serverlessPoolClient = serverlessPoolClient;
        this.logger = logger;
    }

    public async Task AddDatabaseAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $"CREATE DATABASE [{request.DatabaseName}]";

        try
        {
            await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 1801)
        {
            this.logger.LogWarning($"Database already exists={request};", sqlEx);
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to create database={request};", ex);
            throw;
        }
    }

    public async Task AddMasterKeyAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        if (request.MasterKey == null)
        {
            return;
        }

        // Create a master key if it doesn't exist.
        string query = $@"
        USE [{request.DatabaseName}]
        IF EXISTS (SELECT d.is_master_key_encrypted_by_server
        FROM sys.databases AS d
        WHERE d.name = '{request.DatabaseName}' and d.is_master_key_encrypted_by_server = 'false')
        BEGIN
            CREATE MASTER KEY ENCRYPTION BY PASSWORD = '{request.MasterKey.ToPlainString()}'
        END";

        try
        {
            await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 15578)
        {
            this.logger.LogWarning($"Database master key already exists={request};", sqlEx);
        }
        finally
        {
            query = null;
        }
    }

    public async Task AddScopedCredentialAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $@"
        USE [{request.DatabaseName}]
        CREATE DATABASE SCOPED CREDENTIAL {request.ScopedCredential.Name} WITH IDENTITY = '{request.ScopedCredential.Identity}'";

        try
        {
            await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 15530)
        {
            this.logger.LogWarning($"Database scoped credential already exists={request};", sqlEx);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 911)
        {
            this.logger.LogError($"Database does not exist={request};", sqlEx);
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError($"Failed to create database scoped credential={request};", ex);
            throw;
        }
    }

    public async Task AddLoginAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $@"
        USE MASTER
        IF NOT EXISTS 
            (SELECT name  
             FROM master.sys.server_principals
             WHERE name = '{request.LoginName}')
            BEGIN
                CREATE LOGIN [{request.LoginName}] WITH PASSWORD = N'{request.LoginPassword.ToPlainString()}';
            END
        ELSE
            ALTER LOGIN [{request.LoginName}] WITH PASSWORD = N'{request.LoginPassword.ToPlainString()}';
        ";

        try
        {
            await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
        }
        finally
        {
            query = null;
        }
    }

    public async Task AddUserAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $@"
        USE [{request.DatabaseName}]
        CREATE USER [{request.UserName}] FOR LOGIN [{request.LoginName}];";

        try
        {
            await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
        }
        catch (SqlException sqlEx) when (sqlEx.Number == 15023)
        {
            this.logger.LogWarning($"User was already added to the database={request};", sqlEx);
        }
        finally
        {
            query = null;
        }
    }

    public async Task CreateSchemaAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $@"
        USE [{request.DatabaseName}]
        IF NOT EXISTS (SELECT schema_id FROM sys.schemas WHERE name = '{request.SchemaName}')
        BEGIN
            EXEC('CREATE SCHEMA [{request.SchemaName}];');
        END";

        await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
    }

    public async Task GrantCredentialToUserAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $@"
        USE [{request.DatabaseName}]
        GRANT REFERENCES ON DATABASE SCOPED CREDENTIAL::[{request.ScopedCredential.Name}] TO [{request.UserName}];";

        await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
    }

    public async Task GrantUserToSchemaAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        string query = $@"
        USE [{request.DatabaseName}]
        GRANT SELECT ON SCHEMA :: [{request.SchemaName}] TO {request.UserName} WITH GRANT OPTION;";

        await this.serverlessPoolClient.ExecuteCommandAsync(query, cancellationToken);
    }

    public async Task ExecuteSetupScriptAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        await this.ExecuteScriptAsync(request, "setup.sql", cancellationToken);
    }

    public async Task ExecuteSetupRollbackScriptAsync(IDatabaseRequest request, CancellationToken cancellationToken)
    {
        await this.ExecuteScriptAsync(request, "setup-rollback.sql", cancellationToken);
    }

    private async Task ExecuteScriptAsync(IDatabaseRequest request, string scriptName, CancellationToken cancellationToken)
    {
        string currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        string scriptText = await File.ReadAllTextAsync(Path.Combine(currentDirectory, scriptName), cancellationToken);

        var scriptVariables = new Dictionary<string, string>
        {
            { "@databaseName", request.DatabaseName },
            { "@schemaName", request.SchemaName },
            { "@databaseScopedCredential", request.ScopedCredential.Name },
            { "@containerName", request.SchemaName },
            { "@containerUri", $"'{request.DataSourceLocation}'" },
        };

        if (scriptVariables != null)
        {
            foreach (var pair in scriptVariables)
            {
                scriptText = scriptText.Replace(pair.Key, pair.Value);
            }
        }

        await this.serverlessPoolClient.ExecuteCommandAsync(scriptText, cancellationToken);
    }
}
