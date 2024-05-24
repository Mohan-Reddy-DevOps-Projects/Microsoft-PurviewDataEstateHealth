// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

public class MigrateSchedulePayload
{
    public string TenantId { get; set; }

    public string AccountId { get; set; }

    public string RequestId { get; set; }
}
