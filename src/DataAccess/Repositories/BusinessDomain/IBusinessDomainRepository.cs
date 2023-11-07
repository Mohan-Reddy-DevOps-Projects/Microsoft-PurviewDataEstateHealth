// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Business domain repository interface
/// </summary>
public interface IBusinessDomainRepository : 
    ILocationBased<IBusinessDomainRepository>,
    IGetMultipleOperation<IBusinessDomainModel, BusinessDomainQueryCriteria>
{
}
