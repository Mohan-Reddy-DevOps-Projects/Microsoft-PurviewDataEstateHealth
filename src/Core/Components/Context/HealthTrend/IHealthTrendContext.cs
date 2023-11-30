// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Defines properties used for <see cref="IHealthTrendComponent"/> operations.
/// </summary>
public interface IHealthTrendContext : IRootComponentContext
{
    /// <summary>
    /// Gets the domain Id. 
    /// </summary>
    Guid? DomainId { get; internal set; }
}
