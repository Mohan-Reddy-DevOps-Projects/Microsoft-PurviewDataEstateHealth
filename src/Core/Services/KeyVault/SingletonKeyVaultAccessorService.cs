// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Net;
using System.Threading.Tasks;
using global::Azure;
using global::Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Rest.TransientFaultHandling;
using Microsoft.WindowsAzure.ResourceStack.Common.Json;

/// <summary>
/// Access an Azure key vault using a managed identity
/// </summary>
public class SingletonKeyVaultAccessorService : ISingletonKeyVaultAccessorService, IDisposable
{
    private const string DefaultErrorMessage = "Failed to get key vault resource";

    private readonly RetryPolicy<HttpStatusCodeErrorDetectionStrategy> retryPolicy =
        new RetryPolicy<HttpStatusCodeErrorDetectionStrategy>(
            new FixedIntervalRetryStrategy(5, TimeSpan.FromSeconds(15)));

    private readonly IDataEstateHealthLogger logger;

    /// <summary>
    /// Public constructor
    /// </summary>
    public SingletonKeyVaultAccessorService(IDataEstateHealthLogger logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async Task<string> GetSecretAsync(SecretClient secretClient, string secretName)
    {
        if (secretClient == null)
        {
            throw new ArgumentNullException(nameof(secretClient));
        }

        if (string.IsNullOrEmpty(secretName))
        {
            throw new ArgumentNullException(nameof(secretName));
        }

        try
        {
            Response<KeyVaultSecret> response = await secretClient.GetSecretAsync(secretName);

            return response.Value.Value;
        }
        catch (RequestFailedException keyVaultException)
        {
            this.logger.LogError(
                FormattableString.Invariant($"Failed to read secret {secretName} from {secretClient.VaultUri}."),
                keyVaultException);

            if (keyVaultException.Status == (int)HttpStatusCode.NotFound)
            {
                this.logger.LogWarning("Secret not found in key vault.");

                return await Task.FromResult<string>(null);
            }

            throw new ServiceError(
                    ErrorCategory.ServiceError,
                    ErrorCode.KeyVault_GetSecretError.Code,
                    keyVaultException.ToJson())
                .ToException();
        }
        catch (Exception exception)
        {
            this.logger.LogError(
                FormattableString.Invariant(
                    $"Failed to retrieve secret {secretName} from vault {secretClient.VaultUri}."),
                exception);

            throw new ServiceError(
                    ErrorCategory.ServiceError,
                    ErrorCode.KeyVault_GetSecretError.Code,
                    DefaultErrorMessage)
                .ToException();
        }
    }
}
