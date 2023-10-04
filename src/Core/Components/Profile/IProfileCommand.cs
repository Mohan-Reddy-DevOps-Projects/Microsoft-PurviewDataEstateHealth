// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.DataAccess;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines operations for a Service Principal Profile.
/// </summary>
internal interface IProfileCommand : IEntityCreateOperation<IProfileRequest, IProfileModel>,
    IRetrieveEntityByIdOperation<IProfileRequest, IProfileModel>,
    IEntityDeleteOperation<IProfileRequest>
{
}
