// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;

/// <summary>
/// Data Quality Score List Context
/// </summary>
public interface IDataQualityScoreContext : IRootComponentContext
{
    /// <summary>
    /// Business domain id
    /// </summary>
    Guid BusinessDomainId { get; set; }
}
