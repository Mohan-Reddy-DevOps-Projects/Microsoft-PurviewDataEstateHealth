// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.Purview.DataEstateHealth.Common;

/// <inheritdoc cref="IHealthActionContext" />
internal class HealthActionContext : ComponentContext, IHealthActionContext
{
    public Guid BusinessDomainId { get; set; }
}
