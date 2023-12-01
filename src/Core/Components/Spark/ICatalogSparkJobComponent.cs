// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

/// <summary>
/// Catalog spark job component.
/// </summary>
public interface ICatalogSparkJobComponent
{
    /// <summary>
    /// Submit a spark job.
    /// </summary>
    /// <param name="accountServiceModel"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SubmitJob(AccountServiceModel accountServiceModel, CancellationToken cancellationToken);
}
