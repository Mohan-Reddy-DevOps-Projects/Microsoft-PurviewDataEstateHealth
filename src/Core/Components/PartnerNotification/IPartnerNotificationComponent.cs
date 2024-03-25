// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Account notification contract.
/// </summary>
public interface IPartnerNotificationComponent : IComponent<IPartnerNotificationContext>
{
    /// <summary>
    /// Manage create or update notifications
    /// </summary>
    /// <param name="account"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateOrUpdateNotification(AccountServiceModel account, CancellationToken cancellationToken);

    /// <summary>
    /// Deprovision spark jobs
    /// </summary>
    /// <param name="account"></param>
    /// <returns></returns>
    Task DeprovisionSparkJobs(AccountServiceModel account);
}
