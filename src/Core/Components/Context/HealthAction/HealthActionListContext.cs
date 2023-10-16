// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

/// <inheritdoc cref="IHealthActionListContext" />
internal class HealthActionListContext : ComponentContext, IHealthActionListContext
{
    /// <summary>
    /// Business domain id
    /// </summary>
    public Guid? BusinessDomainId { get; set; }
}
