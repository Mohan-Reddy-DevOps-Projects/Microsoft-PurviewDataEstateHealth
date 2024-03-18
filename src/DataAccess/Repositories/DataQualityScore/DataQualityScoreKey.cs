// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Holds information needed to retrieve health action by domainId
/// </summary>
public class DataQualityScoreKey
{
    public DataQualityScoreKey(Guid accountId, string dimension)
    {
        this.AccountId = accountId;
        this.Dimension = dimension;
    }

    /// <summary>
    /// Account id.
    /// </summary>
    public Guid AccountId { get; set; }

    public string Dimension { get; set; }
}
