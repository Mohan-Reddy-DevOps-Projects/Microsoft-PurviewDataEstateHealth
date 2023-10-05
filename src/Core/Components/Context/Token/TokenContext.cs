// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class TokenContext : ComponentContext, ITokenContext
{
    public TokenContext()
    {
    }

    public TokenContext(ITokenContext context) : base(context)
    {
        this.Owner = context.Owner;
    }

    /// <inheritdoc/>
    public string Owner { get; init; }
}
