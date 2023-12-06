// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class RefreshHistoryContext : ComponentContext, IRefreshHistoryContext
{
    public RefreshHistoryContext()
    {
    }

    public RefreshHistoryContext(IRefreshHistoryContext context) : base(context)
    {
        this.DatasetId = context.DatasetId;
        this.Top = context.Top;
    }

    /// <inheritdoc/>
    public Guid DatasetId { get; set; }

    /// <inheritdoc/>
    public int? Top { get; set; }
}
