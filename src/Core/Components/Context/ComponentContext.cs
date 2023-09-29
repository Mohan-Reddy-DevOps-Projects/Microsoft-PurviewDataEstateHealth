// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Linq;

/// <inheritdoc />
internal class ComponentContext
{
    /// <inheritdoc />
    public Guid AccountId { get; init; }

    /// <inheritdoc />
    public Guid TenantId { get; init; }

    /// <inheritdoc />
    public string Location { get; init; }

    /// <inheritdoc />
    public virtual object Key => this.JoinKeys(this.AccountId, this.TenantId);

    /// <inheritdoc />
    protected string JoinKeys(params object[] keys)
    {
        return string.Join("_", keys.Where(k => !string.IsNullOrEmpty(k?.ToString())));
    }
}
