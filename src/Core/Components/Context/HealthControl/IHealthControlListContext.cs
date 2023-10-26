// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
///Health control list context
/// </summary>
public interface IHealthControlListContext : IRootComponentContext
{
    /// <summary>
    /// Health control id
    /// </summary>
    Guid? ControlId { get; set; }
}
