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

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkerService" /> class.
    /// </summary>
    public WorkerService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        IJobDispatcher jobDispatcher = this.serviceProvider.GetRequiredService<IJobDispatcher>();
        await jobDispatcher.Initialize();

        IDataEstateHealthLogger purviewShareLogger = this.serviceProvider.GetRequiredService<IDataEstateHealthLogger>();
        purviewShareLogger.LogInformation("WorkerService initialized and setting up infinite loop.");

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(10000, cancellationToken).ConfigureAwait(false);
            await Task.FromResult(true);
        }
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }
}
