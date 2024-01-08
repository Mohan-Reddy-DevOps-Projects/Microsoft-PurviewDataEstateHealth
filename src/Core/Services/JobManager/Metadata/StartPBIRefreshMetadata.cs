// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Newtonsoft.Json;

internal sealed class StartPBIRefreshMetadata : StagedWorkerJobMetadata
{
    [JsonProperty]
    public IList<RefreshLookup> RefreshLookups { get; set; }

    [JsonProperty]
    public AccountServiceModel Account { get; set; }
}
