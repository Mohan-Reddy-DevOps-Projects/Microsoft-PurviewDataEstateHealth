// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Key to fetch single domain data quality score
/// </summary>
public class DomainDataQualityScoreKey : HealthScoreKey
{
    /// <summary>
    /// Constructor for domain data quality score key
    /// </summary>
    /// <param name="businessDomainId"></param>
    /// <param name="accountId"></param>
    /// <param name="catalogId"></param>
    public DomainDataQualityScoreKey(Guid? businessDomainId, Guid accountId, Guid catalogId)
        : base(businessDomainId, accountId, catalogId)
    {
        this.BusinessDomainId = businessDomainId;
        this.AccountId = accountId;
        this.CatalogId = catalogId;
    }
}
