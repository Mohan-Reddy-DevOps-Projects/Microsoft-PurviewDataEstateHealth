// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Health score list context
/// </summary>
public interface IHealthScoreListContext : IRootComponentContext
{
    /// <summary>
    /// Business domain id
    /// </summary>
    Guid? BusinessDomainId { get; set; }
}
