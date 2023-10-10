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
    public SummaryKey(Guid? domainId)
    {
        this.DomainId = domainId;
    }

    /// <summary>
    /// DomainId.
    /// </summary>
    public Guid? DomainId { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return this.DomainId.ToString();
    }
}
