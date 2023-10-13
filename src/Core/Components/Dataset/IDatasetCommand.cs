// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Defines operations for a dataset.
/// </summary>
internal interface IDatasetCommand : IEntityCreateOperation<IDatasetRequest, Dataset>,
    IRetrieveEntityByIdOperation<IDatasetRequest, Dataset>,
    IEntityDeleteOperation<IDatasetRequest>
{
    /// <summary>
    /// Retrieves a dataset collection.
    /// </summary>
    /// <param name="requestContext">The id used to retrieve the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    Task<Datasets> List(IDatasetRequest requestContext, CancellationToken cancellationToken);

    /// <summary>
    /// Import a dataset.
    /// </summary>
    /// <param name="requestContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Import> Import(IDatasetRequest requestContext, CancellationToken cancellationToken);
}
