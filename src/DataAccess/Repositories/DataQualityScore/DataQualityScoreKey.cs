// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Holds information needed to retrieve health action by domainId
/// </summary>
public class DataQualityScoreKey
{
    public DataQualityScoreKey(Guid accountId, bool queryByDimension = false)
    {
        this.AccountId = accountId;
        this.QueryByDimension = queryByDimension;
    }

    /// <summary>
    /// Account id.
    /// </summary>
    public Guid AccountId { get; set; }

    public bool QueryByDimension { get; set; }
}
