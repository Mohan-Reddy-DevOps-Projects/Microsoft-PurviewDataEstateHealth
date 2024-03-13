// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for health actions.
/// </summary>
public class DataQualityDataProductOutputRecord : BaseRecord
{
    [DataColumn("DataProductID")]
    public string DataProductId { get; set; }

    [DataColumn("BusinessDomainId")]
    public string BusinessDomainId { get; set; }

    [DataColumn("DataProductDisplayName")]
    public string DataProductDisplayName { get; set; }

    [DataColumn("DataProductStatusDisplayName")]
    public string DataProductStatusDisplayName { get; set; }

    [DataColumn("DataProductOwnerIds")]
    public string DataProductOwnerIds { get; set; }

    [DataColumn("result")]
    public string Result { get; set; }
}
