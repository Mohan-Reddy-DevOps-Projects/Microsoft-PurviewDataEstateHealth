// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using global::Azure.Security.KeyVault.Secrets;

/// <summary>
/// Singleton key vault accessor service
/// </summary>
public interface ISingletonKeyVaultAccessorService
{
    /// <summary>
    /// Get a secret from key vault
    /// </summary>
    /// <param name="secretClient">Key Vault secret client</param>
    /// <param name="secretName">Secret name</param>
    /// <returns>Secret value</returns>
    Task<string> GetSecretAsync(SecretClient secretClient, string secretName);
}
