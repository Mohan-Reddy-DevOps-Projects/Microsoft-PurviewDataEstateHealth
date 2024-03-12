// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;
internal sealed class MDQFailedJobMetadata : StagedWorkerJobMetadata
{
    public bool MDQFailedJobProcessed { get; set; }
}
