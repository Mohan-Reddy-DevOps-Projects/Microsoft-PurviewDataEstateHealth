// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using ProcessingStorageModel = Models.ProcessingStorageModel;

/// <summary>
/// Defines the processing storage manager.
/// </summary>
public interface IProcessingStorageManager
{
    /// <summary>
    /// Get processing storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ProcessingStorageModel> Get(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Provision default storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task Provision(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);

    /// <summary>
    /// Delete default storage account.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<DeletionResult> Delete(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);
}
