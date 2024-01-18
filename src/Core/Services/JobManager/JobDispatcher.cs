// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
using Microsoft.DGP.ServiceBasics.Errors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;
using Microsoft.WindowsAzure.Storage;

/// <summary>
/// Job Dispatcher
/// </summary>
public class JobDispatcher : JobDispatcherClient, IJobDispatcher
{
    private readonly IDataEstateHealthRequestLogger logger;

    private readonly EnvironmentConfiguration environmentConfiguration;

    private readonly IServiceProvider scopedServiceProvider;

    /// <summary>
    /// JobDispatcher constructor
    /// </summary>
    private JobDispatcher(
        CloudStorageAccount cloudStorageAccount,
        EnvironmentConfiguration environmentConfiguration,
        IServiceProvider serviceProvider)
        : base(
            cloudStorageAccount,
            environmentConfiguration.Location,
            // TODO(zachmadsen): Pass the job logger here.
            null,
            tableName: "jobdefinitions",
            queueNamePrefix: "jobtriggers",
            requestOptions: null,
            consistencyOptions: null,
            secretThumbprint: null,
            compressionUtility: null,
            factory: new WorkerJobCallbackFactory(serviceProvider),
            instrumentationSource: null)
    {
        this.environmentConfiguration = environmentConfiguration;
        this.logger = serviceProvider.GetRequiredService<IDataEstateHealthRequestLogger>();
        this.scopedServiceProvider = serviceProvider.CreateScope().ServiceProvider;

        // All JobCallbacks should be available in the same assembly as the JobDispatcher.
        this.RegisterJobCallbackAssembly(typeof(JobDispatcher).Assembly);
    }

    /// <summary>
    /// Creates an instance of JobDispatcher asynchronously.
    /// </summary>
    /// <param name="serviceProvider"></param>
    public static async Task<JobDispatcher> CreateAsync(IServiceProvider serviceProvider)
    {
        EnvironmentConfiguration environmentConfiguration = serviceProvider.GetRequiredService<IOptions<EnvironmentConfiguration>>().Value;
        IJobManagementStorageAccountBuilder jobStorageAccountBuilder = serviceProvider.GetRequiredService<IJobManagementStorageAccountBuilder>();
        CloudStorageAccount cloudStorageAccount = await jobStorageAccountBuilder.Build();

        return new JobDispatcher(
            cloudStorageAccount,
            environmentConfiguration,
            serviceProvider);
    }

    /// <summary>
    /// Start the job dispatcher
    /// </summary>
    public async Task Initialize()
    {
        try
        {
            this.logger.LogInformation("Starting job dispatcher.");

            this.Start();

            if (!this.environmentConfiguration.IsDevelopmentEnvironment())
            {
                await this.ProvisionSystemConsistencyJob();
            }

            IJobManager jobManager = this.scopedServiceProvider.GetRequiredService<IJobManager>();

            // All jobs to be provisioned when service comes up...
            await jobManager.ProvisionEventProcessorJob();

            this.logger.LogInformation("Job dispatcher started successfully.");
        }
        catch (Exception exception)
        {
            this.logger.LogCritical("Failed to start Job Dispatcher", exception);

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
