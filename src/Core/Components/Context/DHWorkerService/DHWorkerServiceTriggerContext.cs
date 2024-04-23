// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class DHWorkerServiceTriggerContext : ComponentContext, IDHWorkerServiceTriggerContext
{
    public DHWorkerServiceTriggerContext()
    {
    }

    public DHWorkerServiceTriggerContext(IDHWorkerServiceTriggerContext context) : base(context)
    {
    }
}
