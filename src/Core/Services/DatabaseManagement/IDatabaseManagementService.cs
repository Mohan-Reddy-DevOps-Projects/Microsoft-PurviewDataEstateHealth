// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using System.Threading.Tasks;

internal interface IDatabaseManagementService
{
    Task Initialize(AccountServiceModel accountModel, CancellationToken cancellationToken);

    Task Deprovision(AccountServiceModel accountModel, CancellationToken cancellationToken);
}
