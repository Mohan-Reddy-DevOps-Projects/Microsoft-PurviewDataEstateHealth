// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ResourceStack.Common.BackgroundJobs;

/// <summary>
/// A JobDelegate that results in a faulted job.  This behavior is useful in situations where a job should no longer
/// attempt execution.
/// </summary>
internal class FaultedJobDelegate : JobDelegate
{
    private readonly BackgroundJob backgroundJob;

    public FaultedJobDelegate(BackgroundJob backgroundJob)
    {
        this.backgroundJob = backgroundJob;
    }

    /// <inheritdoc/>
    public override Task<JobExecutionResult> ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new JobExecutionResult()
        {
            Status = JobExecutionStatus.Faulted,
            Message = "Faulting job due to an unrecoverable error that requires further investigation and resolution",
            NextMetadata = this.backgroundJob?.Metadata ?? "{}"
        });
    }
}
