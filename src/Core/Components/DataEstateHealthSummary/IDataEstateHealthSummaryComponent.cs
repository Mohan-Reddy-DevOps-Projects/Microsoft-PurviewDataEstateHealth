// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines operations for a DataEstateHealthSummary object.
/// </summary>
public interface IDataEstateHealthSummaryComponent : IRetrieveEntityOperation<IDataEstateHealthSummaryModel>,
       INavigable<Guid, IDataEstateHealthSummaryComponent>, IComponent<IDataEstateHealthSummaryContext>
{
}
