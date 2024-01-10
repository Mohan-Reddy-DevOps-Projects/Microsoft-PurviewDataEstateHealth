// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using System;

/// <inheritdoc cref="IDataQualityContext" />
internal class DataQualityContext : ComponentContext, IDataQualityContext
{
    public Guid BusinessDomainId { get; set; }

    public Guid DataProductId { get; set; }

    public Guid DataAssetId { get; set; }
}
