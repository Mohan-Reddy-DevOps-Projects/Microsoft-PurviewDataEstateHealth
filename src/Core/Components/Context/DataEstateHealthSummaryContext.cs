// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Components;

internal class DataEstateHealthSummaryContext : IDataEstateHealthSummaryContext, IComponentContext
{
    /// <inheritdoc />
    public Guid AccountId { get; set; }

    /// <inheritdoc />
    public Guid TenantId { get; set; }

    /// <inheritdoc />
    public ServiceVersion Version { get; set; }

    /// <inheritdoc />
    public string Location { get; set; }

    /// <inheritdoc />
    public object Key => this.JoinKeys(this.AccountId, this.TenantId, this.Version);

    /// <inheritdoc />
    protected string JoinKeys(params object[] keys)
    {
        return string.Join("_", keys.Where(k => !string.IsNullOrEmpty(k?.ToString())));
    }
}
