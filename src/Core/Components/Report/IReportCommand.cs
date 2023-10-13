// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.PowerBI.Api.Models;

/// <summary>
/// Defines operations for a report.
/// </summary>
internal interface IReportCommand : IRetrieveEntityByIdOperation<IReportRequest, Report>,
    IEntityDeleteOperation<IReportRequest>
{
    /// <summary>
    /// Retrieves a report collection.
    /// </summary>
    /// <param name="requestContext">The id used to retrieve the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that resolves to the entity information.</returns>
    Task<Reports> List(IReportRequest requestContext, CancellationToken cancellationToken);

    /// <summary>
    /// Provided a shared dataset, create a new report and bind to the shared dataset
    /// </summary>
    /// <param name="targetDataset">The shared dataset to bind to.</param>
    /// <param name="requestContext">The request context used to create a new report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task<Report> Bind(Dataset targetDataset, IDatasetRequest requestContext, CancellationToken cancellationToken);    
}
