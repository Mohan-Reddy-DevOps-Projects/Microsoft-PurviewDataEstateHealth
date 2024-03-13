// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Newtonsoft.Json;

public class DataQualityDataProductOutputEntity : BaseEntity
{
    public DataQualityDataProductOutputEntity()
    {
    }

    [JsonProperty("DataProductID")]
    public string DataProductID { get; set; }

    [JsonProperty("DataProductDisplayName")]
    public string DataProductDisplayName { get; set; }

    [JsonProperty("DataProductStatusDisplayName")]
    public string DataProductStatusDisplayName { get; set; }

    [JsonProperty("BusinessDomainId")]
    public string BusinessDomainId { get; set; }

    [JsonProperty("DataProductOwnerIds")]
    public string DataProductOwnerIds { get; set; }

    [JsonProperty("result")]
    public Dictionary<string, string> Result { get; set; }
}
