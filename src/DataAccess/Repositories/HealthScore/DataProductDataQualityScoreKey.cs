// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Key for fetching data product data quality score
/// </summary>
public class DataProductDataQualityScoreKey : DomainDataQualityScoreKey
{
    /// <summary>
    /// Constructor for data quality score key
    /// </summary>
    /// <param name="dataProductId"></param>
    /// <param name="businessDomainId"></param>
    /// <param name="accountId"></param>
    /// <param name="catalogId"></param>
    public DataProductDataQualityScoreKey(Guid dataProductId, Guid businessDomainId, Guid accountId, Guid catalogId)
        : base(businessDomainId, accountId, catalogId)
    {
        this.DataProductId = dataProductId;
        this.BusinessDomainId = businessDomainId;
        this.AccountId = accountId;
        this.CatalogId = catalogId;
    }

    /// <summary>
    /// Data Product Id
    /// </summary>
    public Guid DataProductId { get; set; }
}
