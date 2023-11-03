// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System.Threading.Tasks;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

internal interface IDatabaseManagementService
{
    Task Initialize(AccountServiceModel accountModel, CancellationToken cancellationToken);
}
