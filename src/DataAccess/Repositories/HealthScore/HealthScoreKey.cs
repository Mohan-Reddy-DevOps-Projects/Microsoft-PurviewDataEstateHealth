// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Holds information needed to retrieve health score by business domainId
/// </summary>
public class HealthScoreKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthScoreKey"/> class.
    /// </summary>
    public HealthScoreKey(Guid? businessDomainId, Guid accountId, Guid catalogId)
    {
        this.BusinessDomainId = businessDomainId;
        this.AccountId = accountId;
        this.CatalogId = catalogId;
    }

    /// <summary>
    /// Business DomainId.
    /// </summary>
    public Guid? BusinessDomainId { get; set; }

    /// <summary>
    /// AccountId
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// CatalogId
    /// </summary>
    public Guid CatalogId { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.BusinessDomainId.ToString();
    }
}
