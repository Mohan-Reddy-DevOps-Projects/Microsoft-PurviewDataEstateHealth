// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;

using Microsoft.Azure.Purview.DataEstateHealth.Models;

internal class ProcessingStorageEntity : TableEntity
{
    public string Properties { get; set; }

    public string TenantId { get; set; }

    public string CatalogId { get; set; }

    public override string ResourceId() => ResourceId(ResourceIds.ProcessingStorage, [this.RowKey.ToString()]);
}
