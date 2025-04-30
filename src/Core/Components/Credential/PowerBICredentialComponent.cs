// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Microsoft.Purview.DataGovernance.SynapseSqlClient;

/// <inheritdoc/>
internal class PowerBICredentialComponent : IPowerBICredentialComponent
{
    private readonly ServerlessPoolAuthConfiguration serverlessAuthConfig;
    private readonly IKeyVaultAccessorService keyVaultAccessor;
    private readonly IDataEstateHealthRequestLogger logger;
    /// <inheritdoc/>
    public PowerBICredentialComponent(IOptions<ServerlessPoolAuthConfiguration> serverlessAuthConfig, IKeyVaultAccessorService keyVaultAccessor, IDataEstateHealthRequestLogger logger)
    {
        this.serverlessAuthConfig = serverlessAuthConfig.Value;
        this.keyVaultAccessor = keyVaultAccessor;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateSynapseDatabaseLoginInfo(PowerBICredential credential, CancellationToken cancellationToken)
    {
        await this.keyVaultAccessor.SetSecretAsync(credential.CredentialSecretKey, credential.Password, cancellationToken);
        this.logger.LogTrace("Reporting.Models:PowerBICredentialComponent:AddOrUpdateSynapseDatabaseLoginInfo executed successfully");
    }

    /// <inheritdoc/>
    public PowerBICredential CreateCredential(Guid accountId, string owner)
    {
        this.logger.LogTrace("Reporting.Models:PowerBICredentialComponent:CreateCredential executed successfully");
        return new PowerBICredential(accountId, this.serverlessAuthConfig.AzureRegion, owner)
        {
            Password = StringExtensions.RandomString(10, 5).ToSecureString()
        };
    }

    /// <inheritdoc/>
    public async Task<PowerBICredential> GetSynapseDatabaseLoginInfo(Guid accountId, string owner, CancellationToken cancellationToken)
    {
        // Login and user name are generated specific to the account. The password is shared across logins for now.
        PowerBICredential credential = new(accountId, this.serverlessAuthConfig.AzureRegion, owner);
        this.logger.LogTrace("Reporting.Models:PowerBICredentialComponent:PowerBI credentials Called");
        KeyVaultSecret secret = await this.keyVaultAccessor.GetSecretAsync(credential.CredentialSecretKey, cancellationToken);
        if (secret == null)
        {
            return null;
        }
        credential.Password = secret.Value.ToSecureString();
        return credential;
    }
    
    /// <inheritdoc/>
    public async Task AddOrUpdateSynapseDatabaseMasterKey(DatabaseMasterKey credential, CancellationToken cancellationToken)
    {
        if (credential != null)
        {
            await this.keyVaultAccessor.SetSecretAsync(this.MasterKeySecretName(), credential.MasterKey, cancellationToken);
        }
    }
    
    /// <inheritdoc/>
    public async Task<DatabaseMasterKey> GetSynapseDatabaseMasterKey(string databaseName, CancellationToken cancellationToken)
    {
        var secret = await this.keyVaultAccessor.GetSecretAsync(this.MasterKeySecretName(), cancellationToken);
        if (secret == null)
        {
            return null;
        }
        return new DatabaseMasterKey
        {
            DatabaseName = databaseName,
            MasterKey = secret.Value.ToSecureString(),
        };
    }

    /// <inheritdoc/>
    public DatabaseMasterKey CreateMasterKey(string databaseName)
    {
        return new DatabaseMasterKey
        {
            DatabaseName = databaseName,
            MasterKey = StringExtensions.RandomString(10, 5).ToSecureString()
        };
    }

    private string MasterKeySecretName() => $"{this.serverlessAuthConfig.AzureRegion}masterKey";
}
