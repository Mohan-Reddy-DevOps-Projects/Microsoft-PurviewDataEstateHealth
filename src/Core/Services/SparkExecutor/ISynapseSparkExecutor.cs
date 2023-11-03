// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

/// <summary>
/// Defines the operations to be performed on the Synapse Spark pool.
/// </summary>
public interface ISynapseSparkExecutor
{
    /// <summary>
    /// Creates the spark pool.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateSparkPool(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);
}
