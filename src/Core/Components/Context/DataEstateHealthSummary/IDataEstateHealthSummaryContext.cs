// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Defines properties used for <see cref="IDataEstateHealthSummaryComponent"/> operations.
/// </summary>
public interface IDataEstateHealthSummaryContext : IRootComponentContext
{
    /// <summary>
    /// Gets the domain Id. 
    /// </summary>
    Guid? DomainId { get; set; }
}
