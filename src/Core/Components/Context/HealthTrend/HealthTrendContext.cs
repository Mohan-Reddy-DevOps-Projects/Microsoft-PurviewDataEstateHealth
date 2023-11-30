// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class HealthTrendContext : ComponentContext, IHealthTrendContext
{
    /// <inheritdoc />
    public Guid? DomainId { get; set; }

    /// <inheritdoc />
    public override string Key => this.JoinKeys(base.Key, this.DomainId);
}
