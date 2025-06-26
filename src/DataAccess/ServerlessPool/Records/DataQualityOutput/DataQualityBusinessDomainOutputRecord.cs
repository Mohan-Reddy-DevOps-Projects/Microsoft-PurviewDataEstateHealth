// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.ServerlessPool.Records.DataQualityOutput;

/// <summary>
/// Intermediate record for business domain data quality output.
/// </summary>
public class DataQualityBusinessDomainOutputRecord : BaseRecord
{
    [DataColumn("BusinessDomainId")]
    public string BusinessDomainId { get; init; }

    [DataColumn("BusinessDomainCriticalDataElementCount")]
    public int? BusinessDomainCriticalDataElementCount { get; init; }

    [DataColumn("result")]
    public string Result { get; init; }
} 