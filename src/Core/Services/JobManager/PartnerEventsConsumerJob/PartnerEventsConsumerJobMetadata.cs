// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Core;

using Newtonsoft.Json;

internal class PartnerEventsConsumerJobMetadata : StagedWorkerJobMetadata
{
    /// <summary>
    /// Flag indicating if data catalog events processed.
    /// </summary>
    [JsonProperty]
    public bool DataCatalogEventsProcessed { get; set; }

    /// <summary>
    /// Flag indicating if data access events processed.
    /// </summary>
    [JsonProperty]
    public bool DataAccessEventsProcessed { get; set; }

    /// <summary>
    /// Flag indicating if data quality events processed.
    /// </summary>
    [JsonProperty]
    public bool DataQualityEventsProcessed { get; set; }

    /// <summary>
    /// Cache for already retrieved processing store accounts.
    /// </summary>
    [JsonProperty]
    public IDictionary<string, string> ProcessingStoresCache { get; set; }
}
