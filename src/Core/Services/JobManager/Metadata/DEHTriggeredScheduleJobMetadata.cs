// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
internal sealed class DEHTriggeredScheduleJobMetadata : StagedWorkerJobMetadata
{
    public DEHTriggeredScheduleStageRunningStatus RunningStatus
    { get; set; } = DEHTriggeredScheduleStageRunningStatus.Ready;
}

internal enum DEHTriggeredScheduleStageRunningStatus
{
    Ready,
    Checked,
    Running,
}