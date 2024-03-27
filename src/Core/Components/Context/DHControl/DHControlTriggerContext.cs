// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class DHControlTriggerContext : ComponentContext, IDHControlTriggerContext
{
    public DHControlTriggerContext()
    {
    }

    public DHControlTriggerContext(IDHControlTriggerContext context) : base(context)
    {
    }
}
