// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;
using Microsoft.DGP.ServiceBasics.Components;

/// <summary>
/// Defines properties used for <see cref="IDataEstateHealthSummaryComponent"/> operations.
/// </summary>
public interface IDataEstateHealthSummaryContext : IComponentContext
{
    /// <summary>
    /// Gets the Account Id the component works within. 
    /// </summary>
    public Guid AccountId { get; }

    /// <summary>
    /// Gets the Tenant Id the component works within. 
    /// </summary>
    public Guid TenantId { get; }

    /// <summary>
    /// Gets the service version the component works within.
    /// </summary>
    public ServiceVersion Version { get; }

    /// <summary>
    /// Gets the location the component works within.
    /// </summary>
    public string Location { get; }

}
