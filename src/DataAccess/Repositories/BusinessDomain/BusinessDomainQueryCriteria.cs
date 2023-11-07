// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Defines business domain search criteria.
/// </summary>
public class BusinessDomainQueryCriteria
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessDomainQueryCriteria"/> class.
    /// </summary>
    public BusinessDomainQueryCriteria(Guid catalogId, Guid accountId)
    {
        this.CatalogId = catalogId;
        this.AccountId = accountId;
    }

    /// <summary>
    /// Catalog id.
    /// </summary>
    public Guid CatalogId { get; set; }

    /// <summary>
    /// Account id.
    /// </summary>
    public Guid AccountId { get; set; } 
}
