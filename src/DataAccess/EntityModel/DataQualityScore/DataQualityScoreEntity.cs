// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.DataAccess;

using Microsoft.Azure.Purview.DataEstateHealth.Models;
using Newtonsoft.Json;

public class DataQualityScoreEntity : BaseEntity
{
    public DataQualityScoreEntity()
    {
    }

    public DataQualityScoreEntity(DataQualityScoreEntity entity)
    {
        this.Id = entity.Id;
    }

    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("dataAssetId")]
    public Guid DataAssetId { get; set; }

    [JsonProperty("businessDomainId")]
    public Guid BusinessDomainId { get; set; }

    [JsonProperty("dataProductId")]
    public Guid DataProductId { get; set; }

    [JsonProperty("dqJobId")]
    public Guid DQJobId { get; set; }

    [JsonProperty("executionTime")]
    public DateTime ExecutionTime { get; set; }

    [JsonProperty("score")]
    public double Score { get; set; }

    [JsonProperty("dataProductOwners")]
    public IEnumerable<string> DataProductOwners { get; set; }

    [JsonProperty("dataProductStatus")]
    public string DataProductStatus { get; set; }

    [JsonProperty("qualityDimension")]
    public string QualityDimension { get; set; }
}
