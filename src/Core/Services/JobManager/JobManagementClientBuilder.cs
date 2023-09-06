// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage;

/// <inheritdoc />
public class JobManagementClientBuilder : IJobManagementClientBuilder
{

    private readonly IOptions<JobManagerConfiguration> jobManagerConfiguration;

    private readonly IOptions<EnvironmentConfiguration> environmentConfiguration;

    private readonly ISingletonStorageAccessorService singletonStorageAccessorService;


    /// <summary>
    /// JobManagementClientBuilder initialization
    /// </summary>
    public JobManagementClientBuilder(
        IOptions<JobManagerConfiguration> jobManagerConfiguration,
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        ISingletonStorageAccessorService singletonStorageAccessorService)
    {
        this.environmentConfiguration = environmentConfiguration;
        this.jobManagerConfiguration = jobManagerConfiguration;
        this.singletonStorageAccessorService = singletonStorageAccessorService;
    }

    /// <inheritdoc />
    public JobManagementClient Build()
    {
        var dataConsistencyOptions = new StorageConsistencyOptions
        {
            ShardingEnabled = false,
            ReplicationEnabled = false
        };

        return new JobManagementClient(
            this.singletonStorageAccessorService
                .GetConnectionString(this.jobManagerConfiguration.Value.BackgroundJobStorageResourceId)
                .GetAwaiter()
                .GetResult(),
            this.environmentConfiguration.Value.Location,
            null,
            "jobdefinitions",
            "jobtriggers",
            null,
            dataConsistencyOptions,
            secretThumbprint: null,
            compressionUtility: null,
            notificationChannel: null);
    }
}
