// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Holds information needed to retrieve health action by domainId
/// </summary>
public class HealthActionKey
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HealthActionKey"/> class.
    /// </summary>
    public HealthActionKey(Guid businessDomainId)
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
