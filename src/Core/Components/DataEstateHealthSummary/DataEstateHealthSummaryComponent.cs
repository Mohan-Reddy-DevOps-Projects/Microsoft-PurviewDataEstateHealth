// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Microsoft.DGP.ServiceBasics.Components;

/// <inheritdoc />
[Component(typeof(IDataEstateHealthSummaryComponent))]
internal class DataEstateHealthSummaryComponent : BaseComponent<IDataEstateHealthSummaryContext>, IDataEstateHealthSummaryComponent
{
    public DataEstateHealthSummaryComponent(DataEstateHealthSummaryContext context, int version) : base(context, version)
    {
    }

    public IDataEstateHealthSummaryComponent ById(Guid id)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<IDataEstateHealthSummaryModel> Get()
    {
        throw new NotImplementedException();
    }
}

