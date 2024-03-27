// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Purview.DataGovernance.Reporting.Models;

public interface IHealthProfileCommand : IEntityCreateOperation<ProfileKey, IProfileModel>,
    IRetrieveEntityByIdOperation<ProfileKey, IProfileModel>,
    IEntityDeleteOperation<ProfileKey>
{
}