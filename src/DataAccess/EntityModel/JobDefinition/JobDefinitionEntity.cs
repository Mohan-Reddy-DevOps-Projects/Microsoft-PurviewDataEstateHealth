// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class JobDefinitionEntity : TableEntity
{
    public string JobPartition { get; set; }

    public string JobId { get; set; }

    public string Callback { get; set; }

    public string LastExecutionStatus { get; set; }

    public override string ResourceId() => ResourceId(ResourceIds.JobDefinition, [this.RowKey.ToString()]);
}
