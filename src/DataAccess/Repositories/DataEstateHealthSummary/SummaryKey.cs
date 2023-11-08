// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Holds information needed to retrieve summary by Id
/// </summary>
public class SummaryKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SummaryKey"/> class.
    /// </summary>
    public SummaryKey(Guid? domainId, Guid accountId, Guid catalogId)
    {
        this.DomainId = domainId;
        this.AccountId = accountId;
        this.CatalogId = catalogId;
    }

    /// <summary>
    /// DomainId.
    /// </summary>
    public Guid? DomainId { get; set; }

    /// <summary>
    /// Account id.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Catalog id.
    /// </summary>
    public Guid CatalogId { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.DomainId.ToString();
    }
}
