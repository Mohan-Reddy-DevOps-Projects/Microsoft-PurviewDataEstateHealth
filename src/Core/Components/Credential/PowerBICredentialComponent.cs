// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Purview.DataEstateHealth.Common.Extensions;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.Extensions.Options;

/// <inheritdoc/>
internal class PowerBICredentialComponent : IPowerBICredentialComponent
{
    private readonly ServerlessPoolAuthConfiguration serverlessAuthConfig;
    private readonly IKeyVaultAccessorService keyVaultAccessor;
    
    /// <inheritdoc/>
    public PowerBICredentialComponent(IOptions<ServerlessPoolAuthConfiguration> serverlessAuthConfig, IKeyVaultAccessorService keyVaultAccessor)
    {
        this.serverlessAuthConfig = serverlessAuthConfig.Value;
        this.keyVaultAccessor = keyVaultAccessor;
    }

    /// <inheritdoc/>
    public async Task AddOrUpdateSynapseDatabaseLoginInfo(PowerBICredential credential, CancellationToken cancellationToken)
    {
        await this.keyVaultAccessor.SetSecretAsync(credential.CredentialSecretKey, credential.Password, cancellationToken);
    }
    
    /// <inheritdoc/>
    public PowerBICredential CreateCredential(Guid accountId, string owner)
    {
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
