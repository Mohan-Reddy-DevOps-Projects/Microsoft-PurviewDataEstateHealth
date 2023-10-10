// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;

/// <summary>
/// Health action context
/// </summary>
public interface IHealthActionContext : IHealthActionListContext
{
    /// <summary>
    /// Business domain id
    /// </summary>
    Guid BusinessDomainId { get; set; }
}
