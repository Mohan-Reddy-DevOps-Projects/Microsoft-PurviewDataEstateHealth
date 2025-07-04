﻿// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <summary>
/// Health action list context
/// </summary>
public interface IHealthActionListContext : IRootComponentContext
{
    /// <summary>
    /// Business domain id
    /// </summary>
    Guid? BusinessDomainId { get; set; }
}
