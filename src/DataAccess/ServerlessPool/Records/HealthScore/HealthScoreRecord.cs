// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Intermediate record for health score for one business domain.
/// </summary>
internal class HealthScoreRecord : HealthScoreRecordForAllBusinessDomains
{
    /// <summary>
    /// Business domain Id.
    /// </summary>
    [DataColumn("BusinessDomainId")]
    public Guid BusinessDomainId { get; set; }
}
