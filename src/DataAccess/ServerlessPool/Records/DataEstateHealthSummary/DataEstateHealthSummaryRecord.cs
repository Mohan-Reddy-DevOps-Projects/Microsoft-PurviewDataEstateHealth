// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using System;

/// <summary>
/// Intermediate record for data estate health summary for one business domain.
/// </summary>
internal class DataEstateHealthSummaryRecord : DataEstateHealthSummaryRecordForAllBusinessDomains
{
    /// <summary>
    /// Business domain Id.
    /// </summary>
    [DataColumn("BusinessDomainId")]
    public Guid BusinessDomainId { get; set; }
}
