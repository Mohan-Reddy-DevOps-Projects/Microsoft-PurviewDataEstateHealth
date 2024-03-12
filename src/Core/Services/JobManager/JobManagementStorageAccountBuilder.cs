// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Threading.Tasks;

/// <summary>
/// JobManagementStorageAccountBuilder
/// </summary>
public class JobManagementStorageAccountBuilder : IJobManagementStorageAccountBuilder
{
    private readonly JobConfiguration jobConfiguration;

    private readonly EnvironmentConfiguration environmentConfiguration;

    private readonly IStorageCredentialsProvider storageCredentialsProvider;

    /// <summary>
    /// JobStorageAccountBuilder initialization
    /// </summary>
    public JobManagementStorageAccountBuilder(
        IOptions<JobConfiguration> jobConfiguration,
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        IStorageCredentialsProvider storageCredentialsProvider)
    {
        this.environmentConfiguration = environmentConfiguration.Value;
        this.jobConfiguration = jobConfiguration.Value;
        this.storageCredentialsProvider = storageCredentialsProvider;
    }

    /// <inheritdoc />
    public async Task<CloudStorageAccount> Build()
    {
        if (this.environmentConfiguration.IsDevelopmentEnvironment())
        {
            return CloudStorageAccount.Parse(this.jobConfiguration.StorageAccountName);
        }
        else
        {
            StorageCredentials storageCredentials = await this.storageCredentialsProvider.GetRenewableCredentialsAsync();
            return new CloudStorageAccount(
                storageCredentials: storageCredentials,
                this.jobConfiguration.StorageAccountName,
                endpointSuffix: this.environmentConfiguration.AzureEnvironment.StorageEndpointSuffix,
                useHttps: true
            );
        }
    }
}
