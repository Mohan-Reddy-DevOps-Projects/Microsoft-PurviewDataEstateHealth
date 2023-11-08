// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel;

internal class SparkPoolEntity : TableEntity
{
    public string Properties { get; set; }

    public string TenantId { get; set; }
}
