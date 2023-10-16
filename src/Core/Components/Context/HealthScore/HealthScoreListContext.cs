// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <inheritdoc cref="IHealthScoreListContext" />
internal class HealthScoreListContext : ComponentContext, IHealthScoreListContext
{
    public Guid? BusinessDomainId { get; set; }
}
