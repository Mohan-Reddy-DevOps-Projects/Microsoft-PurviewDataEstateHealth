// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

/// <summary>
/// Defines the business domain model
/// </summary>
public interface IBusinessDomainModel
{
    /// <summary>
    /// Business domain name
    /// </summary>
    string BusinessDomainName { get; }

    /// <summary>
    /// Business domain Id
    /// </summary>
    Guid BusinessDomainId { get; }
}
