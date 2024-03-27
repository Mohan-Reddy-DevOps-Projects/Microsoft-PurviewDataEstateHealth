// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Account notification contract.
/// </summary>
public interface IDHControlTriggerComponent : IComponent<IDHControlTriggerContext>
{
    /// <summary>
    /// Manage create or update notifications
    /// </summary>
    /// <param name="account"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RefreshPowerBI(AccountServiceModel account, CancellationToken cancellationToken);
}
