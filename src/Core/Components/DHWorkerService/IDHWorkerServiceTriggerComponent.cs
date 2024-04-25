// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Account notification contract.
/// </summary>
public interface IDHWorkerServiceTriggerComponent : IComponent<IDHWorkerServiceTriggerContext>
{
    /// <summary>
    /// Manage create or update notifications
    /// </summary>
    /// <param name="account"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RefreshPowerBI(AccountServiceModel account, CancellationToken cancellationToken);

    /// <summary>
    /// Trigger a background job.
    /// </summary>
    /// <param name="jobPartition"></param>
    /// <param name="jobId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task TriggerBackgroundJob(string jobPartition, string jobId, CancellationToken cancellationToken);

    /// <summary>
    /// Trigger a background job.
    /// </summary>
    /// <param name="jobPartition"></param>
    /// <param name="jobId"></param>
    /// <returns></returns>
    Task<Dictionary<string, string>> GetBackgroundJob(string jobPartition, string jobId);
}
