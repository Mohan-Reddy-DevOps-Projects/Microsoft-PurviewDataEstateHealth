// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the business domains class
/// </summary>
public class BusinessDomain
{
    /// <summary>
    /// Name of the business domain
    /// </summary>
    public string BusinessDomainName { get; set; }

    /// <summary>
    /// Id of the business domain
    /// </summary>
    public Guid BusinessDomainId { get; set; }
}
