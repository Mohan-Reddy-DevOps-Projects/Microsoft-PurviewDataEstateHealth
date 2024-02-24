// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Purview.DataGovernance.Reporting;
using Microsoft.Purview.DataGovernance.Reporting.Models;

internal interface IRefreshComponent
{
    Task<IList<RefreshDetailsModel>> GetRefreshStatus(IList<RefreshLookup> refreshLookups, CancellationToken cancellationToken);
    Task<IList<RefreshLookup>> RefreshDatasets(Guid accountId, CancellationToken cancellationToken);
    Task<IList<RefreshLookup>> RefreshDatasets(IDatasetRequest[] requests, CancellationToken cancellationToken);
}
