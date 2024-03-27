// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

internal sealed class ActionCleanUpJobMetadata : StagedWorkerJobMetadata
{
    public AccountServiceModel AccountServiceModel { get; set; }
    public bool ActionCleanUpCompleted { get; set; }
}
