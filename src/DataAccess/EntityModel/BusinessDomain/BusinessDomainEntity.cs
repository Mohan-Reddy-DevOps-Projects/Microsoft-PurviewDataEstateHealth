// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Business domain entity.
/// </summary>
public class BusinessDomainEntity
{
    /// <summary>
    /// Gets or sets the business domain id.
    /// </summary>
    [Key]
    public Guid BusinessDomainId { get; set; }

    /// <summary>
    /// Gets or sets the business domain name.
    /// </summary>
    public string BusinessDomainDisplayName { get; set; }
}

