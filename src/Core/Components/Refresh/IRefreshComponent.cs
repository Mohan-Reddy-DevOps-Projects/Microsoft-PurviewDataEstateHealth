// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal interface IRefreshComponent
{
    Task<IList<RefreshDetailsModel>> GetRefreshStatus(IList<RefreshLookup> refreshLookups, CancellationToken cancellationToken);
    Task<IList<RefreshLookup>> RefreshDatasets(Guid accountId, CancellationToken cancellationToken);
}
