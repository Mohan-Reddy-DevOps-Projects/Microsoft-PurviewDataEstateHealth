// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

internal class PartnerEventsConsumerJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Flag indicating if data catalog events processed.
    /// </summary>
    public bool DataCatalogEventsProcessed;

    /// <summary>
    /// Flag indicating if data access events processed.
    /// </summary>
    public bool DataAccessEventsProcessed;

    /// <summary>
    /// Flag indicating if data quality events processed.
    /// </summary>
    public bool DataQualityEventsProcessed;
}
