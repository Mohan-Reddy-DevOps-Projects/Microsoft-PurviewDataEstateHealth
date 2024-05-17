// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
internal sealed class DEHScheduleJobMetadata : StagedWorkerJobMetadata
{
    public string ScheduleTenantId { get; set; }

    public string ScheduleAccountId { get; set; }
}
