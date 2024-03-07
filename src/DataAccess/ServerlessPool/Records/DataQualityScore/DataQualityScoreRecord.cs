// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for health actions.
/// </summary>
public class DataQualityScoreRecord : BaseRecord
{
    [DataColumn("DataAssetId")]
    public Guid DataAssetId { get; set; }

    [DataColumn("BusinessDomainId")]
    public Guid BusinessDomainId { get; set; }

    [DataColumn("DataProductId")]
    public Guid DataProductId { get; set; }

    [DataColumn("DQJobId")]
    public Guid DQJobId { get; set; }

    [DataColumn("ExecutionTime")]
    public DateTime ExecutionTime { get; set; }

    [DataColumn("Score")]
    public double Score { get; set; }

    [DataColumn("DataProductOwnerIds")]
    public string DataProductOwnerIds { get; set; }

    [DataColumn("DataProductStatus")]
    public string DataProductStatus { get; set; }
}
