// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Threading.Tasks;

internal interface IDatabaseManagementService
{
    Task Initialize(Guid accountId, CancellationToken cancellationToken);
}
