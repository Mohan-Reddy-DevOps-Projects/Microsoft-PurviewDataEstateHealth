// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class MDQFailedJobEntity : TableEntity
{
    public string TenantId { get; set; }

    public string AccountId { get; set; }

    public string DQJobId { get; set; }

    public string JobStatus { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public override string ResourceId() => ResourceId(ResourceIds.MDQFailedJob, [this.RowKey.ToString()]);
}
