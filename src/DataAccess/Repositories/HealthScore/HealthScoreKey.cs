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
    public HealthScoreKey(Guid businessDomainId)
    {
        this.BusinessDomainId = businessDomainId;
    }

    /// <summary>
    ///Business DomainId.
    /// </summary>
    public Guid BusinessDomainId { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.BusinessDomainId.ToString();
    }
}
