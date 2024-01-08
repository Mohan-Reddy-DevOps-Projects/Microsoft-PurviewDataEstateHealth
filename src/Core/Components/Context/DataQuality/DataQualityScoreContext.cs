// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using System;

/// <inheritdoc cref="IDataQualityScoreContext" />
internal class DataQualityScoreContext : ComponentContext, IDataQualityScoreContext
{
    public Guid BusinessDomainId { get; set; }
}
