// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.Text.Json;

/// <summary>
/// Holds information needed to retrieve health controls for an account.
/// </summary>
public class HealthControlsKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthControlsKey"/> class.
    /// </summary>
    public HealthControlsKey(Guid accountId, Guid catalogId)
    {
        this.AccountId = accountId;
        this.CatalogId = catalogId;
    }

    /// <summary>
    /// AccountId.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// CatalogId
    /// </summary>
    public Guid CatalogId { get; set; }


    /// <inheritdoc />
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
