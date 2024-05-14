// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

public class TriggeredSchedulePayload
{
    public string TenantId { get; set; }

    public string AccountId { get; set; }

    public string ControlId { get; set; }

    public string Operator { get; set; }

    public string TriggerType { get; set; }

    public string RequestId { get; set; }
}
