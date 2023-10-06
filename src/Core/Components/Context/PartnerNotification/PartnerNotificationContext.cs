// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class PartnerNotificationContext : ComponentContext, IPartnerNotificationContext
{
    public PartnerNotificationContext()
    {
    }

    public PartnerNotificationContext(IPartnerNotificationContext context) : base(context)
    {
    }
}
