// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.Azure.Purview.Share.Common;
using Microsoft.Azure.Purview.Share.Configurations;
using Microsoft.Azure.Purview.Share.Core.V1;
using Microsoft.Azure.Purview.Share.Core.V2;
using Microsoft.Azure.Purview.Share.Logger;
using Microsoft.Azure.Purview.Share.Models;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.ResourceStack.Common.EventSources;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage;
using Microsoft.WindowsAzure.ResourceStack.Common.Utilities;

/// <summary>
/// Job Dispatcher
/// </summary>
public class JobDispatcher : JobDispatcherClient, IJobDispatcher
{
    private readonly IDataEstateHealthLogger dataEstateHealthLogger;

    private readonly IOptions<EnvironmentConfiguration> environmentConfiguration;

    private readonly string jobLocation;

    private readonly JobManagementClient jobManagementClient;

    /// <summary>
    /// JobDispatcher constructor
    /// </summary>
    public JobDispatcher(
        IOptions<JobManagerConfiguration> jobManagerConfiguration,
        IOptions<EnvironmentConfiguration> environmentConfiguration,
        IBackgroundJobsEventSource jobLogger,
        IDataEstateHealthLogger dataEstateHealthLogger,
        ISingletonStorageAccessorService singletonStorageAccessorService,
        IJobManagementClientBuilder jobManagementClientBuilder,
        IServiceProvider serviceProvider = null)
        : base(
            singletonStorageAccessorService
                .GetConnectionString(jobManagerConfiguration.Value.BackgroundJobStorageResourceId)
                .GetAwaiter()
                .GetResult(),
            environmentConfiguration.Value.Location,
            jobLogger,
            tableName: "jobdefinitions",
            queueNamePrefix: "jobtriggers",
            requestOptions: null,
            consistencyOptions: null,
            secretThumbprint: null,
            compressionUtility: null,
            factory: new WorkerJobCallbackFactory(serviceProvider),
            instrumentationSource: null)
    {
        this.dataEstateHealthLogger = dataEstateHealthLogger;
        this.jobLocation = environmentConfiguration.Value.Location;
        this.environmentConfiguration = environmentConfiguration;
        this.jobManagementClient = jobManagementClientBuilder.Build();

        // All JobCallbacks should be available in the same assembly as the JobDispatcher
        this.RegisterJobCallbackAssembly(typeof(JobDispatcher).Assembly);
    }

    /// <summary>
    /// Start the job dispatcher
    /// </summary>
    public async Task Initialize()
    {
        try
        {
            this.dataEstateHealthLogger.LogInformation("Starting job dispatcher.");

            this.Start();

            await this.ProvisionSystemConsistencyJob();

            this.dataEstateHealthLogger.LogInformation("Job dispatcher started successfully.");
        }
        catch (Exception exception)
        {
            this.dataEstateHealthLogger.LogCritical("Failed to start Job Dispatcher", exception);

            throw new ServiceException(
                    new ServiceError(
                        ErrorCategory.ServiceError,
                        ErrorCode.StartupError.Code,
                        exception.StackTrace),
                    exception);
        }
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Dispose overload
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.StopAsync();
        }
    }
}
