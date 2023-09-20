// -----------------------------------------------------------------------
//  <copyright file="ComponentContext.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;
using System.Linq;
using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <inheritdoc />
internal class ComponentContext
{
    /// <inheritdoc />
    public Guid AccountId { get; init; }

    /// <inheritdoc />
    public Guid TenantId { get; init; }

    /// <inheritdoc />
    public ServiceVersion Version { get; init; }

    /// <inheritdoc />
    public string Location { get; init; }

    /// <inheritdoc />
    public virtual object Key => this.JoinKeys(this.AccountId, this.TenantId, this.Version);

    /// <inheritdoc />
    protected string JoinKeys(params object[] keys)
    {
        return string.Join("_", keys.Where(k => !string.IsNullOrEmpty(k?.ToString())));
    }
}
