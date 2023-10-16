// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines a contract for managing data estate health summary collections.
/// </summary>
public interface IDataEstateHealthSummaryComponent : IRetrieveEntityOperation<IDataEstateHealthSummaryModel>,
    INavigable<Guid, IDataEstateHealthSummaryComponent>, IComponent<IDataEstateHealthSummaryContext>
{
}
