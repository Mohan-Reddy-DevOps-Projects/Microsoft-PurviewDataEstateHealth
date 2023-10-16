// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.ApiService;

using System.ComponentModel;

/// <summary>
/// A Business Domains class.
/// </summary>
public class BusinessDomain
{
    /// <summary>
    /// Name of the business domain
    /// </summary>
    [ReadOnly(true)]
    public string BusinessDomainName { get; internal set; }

    /// <summary>
    /// Id of the business domain
    /// </summary>
    [ReadOnly(true)]
    public Guid BusinessDomainId { get; internal set; }
}
