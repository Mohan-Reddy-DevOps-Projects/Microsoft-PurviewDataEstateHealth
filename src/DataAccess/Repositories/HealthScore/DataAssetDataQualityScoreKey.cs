// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Key for fetching data asset data quality score
/// </summary>
public class DataAssetDataQualityScoreKey : DataProductDataQualityScoreKey
{
    /// <summary>
    /// Constructor for data asset data quality score key
    /// </summary>
    /// <param name="dataAssetId"></param>
    /// <param name="dataProductId"></param>
    /// <param name="businessDomainId"></param>
    /// <param name="accountId"></param>
    /// <param name="catalogId"></param>
    public DataAssetDataQualityScoreKey(Guid dataAssetId, Guid dataProductId, Guid businessDomainId, Guid accountId, Guid catalogId)
        : base(dataProductId, businessDomainId, accountId, catalogId)
    {
        this.DataAssetId = dataAssetId;
        this.DataProductId = dataProductId;
        this.BusinessDomainId = businessDomainId;
        this.AccountId = accountId;
        this.CatalogId = catalogId;
    }

    /// <summary>
    /// Data Asset Id
    /// </summary>
    public Guid DataAssetId { get; set; }
}

