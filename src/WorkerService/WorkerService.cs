// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.WorkerService;

using Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.Purview.DataEstateHealth.Loggers;

/// <summary>
/// Worker service implementation.
/// </summary>
public class WorkerService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IDataEstateHealthRequestLogger logger;
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly ManualResetEvent runCompleteEvent = new(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerService" /> class.
    /// </summary>
    public WorkerService(IServiceProvider serviceProvider, IDataEstateHealthRequestLogger logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("WorkerService is running");
        try
        {
            JobDispatcher jobDispatcher = await JobDispatcher.CreateAsync(this.serviceProvider);
            await jobDispatcher.Initialize();

            this.logger.LogInformation("WorkerService initialized and setting up infinite loop.");

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
                await Task.FromResult(true);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError("Exception occurred in Dispatcher", ex);
            throw;
        }
        finally
        {
            this.runCompleteEvent.Set();
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        this.cancellationTokenSource.Cancel();
        this.runCompleteEvent.WaitOne();
        await base.StopAsync(cancellationToken);
    }
}
