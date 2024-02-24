// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class WorkspaceContext : ComponentContext, IWorkspaceContext
{
    public WorkspaceContext()
    {
    }

    public WorkspaceContext(IRootComponentContext context) : base(context)
    {
    }

    public WorkspaceContext(IWorkspaceContext context) : base(context)
    {
        this.ProfileId = context.ProfileId;
    }

    /// <inheritdoc/>
    public Guid ProfileId { get; init; }
}
