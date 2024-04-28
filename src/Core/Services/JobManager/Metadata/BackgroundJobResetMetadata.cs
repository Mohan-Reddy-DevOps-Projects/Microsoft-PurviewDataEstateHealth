// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
using Microsoft.Azure.ProjectBabylon.Metadata.Models;

internal sealed class BackgroundJobResetMetadata : StagedWorkerJobMetadata
{
    public AccountServiceModel AccountServiceModel { get; set; }

    public bool IsCompleted { get; set; }
}
