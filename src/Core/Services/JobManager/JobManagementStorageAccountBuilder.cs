// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

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
        if (environmentConfiguration.IsDevelopmentEnvironment())
        {
            return CloudStorageAccount.Parse(jobConfiguration.StorageAccountName);
        }
        else
        {
            StorageCredentials storageCredentials = await storageCredentialsProvider.GetRenewableCredentialsAsync();
            return new CloudStorageAccount(
                storageCredentials: storageCredentials,
                jobConfiguration.StorageAccountName,
                endpointSuffix: environmentConfiguration.AzureEnvironment.StorageEndpointSuffix,
                useHttps: true
            );
        }
    }
}
