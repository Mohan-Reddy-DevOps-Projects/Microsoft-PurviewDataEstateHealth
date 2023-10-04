// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.DataAccess;

/// <summary>
/// DataEstateHealth Summary repostory interface
/// </summary>
public interface IDataEstateHealthSummaryRepository : IGetSingleOperation<IDataEstateHealthSummaryModel, SummaryKey>, ILocationBased<IDataEstateHealthSummaryRepository>
{
}
