// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Account notification contract.
/// </summary>
public interface IPartnerNotificationComponent : IComponent<IPartnerNotificationContext>
{
    /// <summary>
    /// Manage create or update notifications
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateOrUpdateNotification(CancellationToken cancellationToken);
}
