// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ProvisioningService;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

/// <summary>
/// Defines the processing storage manager.
/// </summary>
public interface IProcessingStorageManager
{
    /// <summary>
    /// Provision default storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Provision(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);
}
