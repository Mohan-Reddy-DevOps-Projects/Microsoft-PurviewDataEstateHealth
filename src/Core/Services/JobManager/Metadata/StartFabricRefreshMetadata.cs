// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Microsoft.Azure.ProjectBabylon.Metadata.Models;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Purview.DataGovernance.Reporting.Models;
using Newtonsoft.Json;

internal sealed class StartFabricRefreshMetadata : StagedWorkerJobMetadata
{
    [JsonProperty]
    public IList<RefreshLookup> RefreshLookups { get; set; }

    [JsonProperty]
    public AccountServiceModel Account { get; set; }

    /// <summary>
    /// The datasets that were upgraded and should be refreshed.
    /// </summary>
    public Dictionary<Guid, List<Dataset>> DatasetUpgrades { get; set; }

    /// <summary>
    /// The profile id.
    /// </summary>
    public Guid ProfileId { get; set; }

    /// <summary>
    /// The workspace id.
    /// </summary>
    public Guid WorkspaceId { get; set; }

    /// <summary>
    /// True if the report refresh completed.
    /// </summary>
    public bool ReportRefreshCompleted { get; set; }
}
