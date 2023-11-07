// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

/// <summary>
/// Intermediate record for Business domain.
/// </summary>
public class BusinessDomainRecord : BaseRecord
{
    /// <summary>
    /// Business domain id.
    /// </summary>
    [DataColumn("BusinessDomainId")]
    public Guid BusinessDomainId { get; set; }

    /// <summary>
    /// Business domain display name.
    /// </summary>
    [DataColumn("BusinessDomainDisplayName")]
    public string BusinessDomainDisplayName { get; set; }
}
