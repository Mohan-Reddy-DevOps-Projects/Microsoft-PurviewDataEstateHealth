// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;
using Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Holds information needed to retrieve trends 
/// </summary>
public class HealthTrendKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthTrendKey"/> class.
    /// </summary>
    public HealthTrendKey(Guid? domainId, Guid accountId, Guid catalogId, TrendKind trendKind)
    {
        this.DomainId = domainId;
        this.AccountId = accountId;
        this.CatalogId = catalogId;
        this.TrendKind = trendKind;
    }

    /// <summary>
    /// The range of days to calculate the trend over.
    /// </summary>
    public const int TrendDuration = 30;

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

    /// <summary>
    /// Trend Kind.
    /// </summary>
    public TrendKind TrendKind { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.DomainId.ToString();
    }
}
