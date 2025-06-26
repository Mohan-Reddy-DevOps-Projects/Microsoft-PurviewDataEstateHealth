// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess.EntityModel.DataQualityOutput;

using Newtonsoft.Json;

public class DataQualityBusinessDomainOutputEntity : BaseEntity
{
    [JsonProperty(nameof(BusinessDomainId))]
    public string BusinessDomainId { get; set; }

    [JsonProperty(nameof(BusinessDomainCriticalDataElementCount))]
    public int? BusinessDomainCriticalDataElementCount { get; set; }

    [JsonProperty("result")]
    public Dictionary<string, string> Result { get; set; }
} 