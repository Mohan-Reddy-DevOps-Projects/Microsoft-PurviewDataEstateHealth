// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------
namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Security;
using System.Threading.Tasks;
using global::Azure.Security.KeyVault.Secrets;

/// <summary>
/// Key vault accessor service
/// </summary>
public interface IKeyVaultAccessorService
{
    /// <summary>
    /// Get a secret from key vault
    /// </summary>
    /// <param name="secretName">Secret name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="version">Secret version</param>
    /// <returns>Secret value</returns>
    Task<KeyVaultSecret> GetSecretAsync(string secretName, CancellationToken cancellationToken, string version = null);

    /// <summary>
    /// Set a secret in key vault
    /// </summary>
    /// <param name="secretName">Secret name</param>
    /// <param name="secretValue">Secret value</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns></returns>
    Task<KeyVaultSecret> SetSecretAsync(string secretName, SecureString secretValue, CancellationToken cancellationToken);
}
