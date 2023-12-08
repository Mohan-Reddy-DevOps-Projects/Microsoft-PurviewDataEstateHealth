// -----------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------

namespace Microsoft.Azure.Purview.DataEstateHealth.Models;

using Microsoft.Data.DeltaLake.Types;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Newtonsoft.Json;

// !!! TODO (TASK 2782067) Find scalable alternative to these classes.

/// <summary>
/// DQ job result values.
/// </summary>
public class JobResultValuesEventHubEntityModel
{
    /// <summary>
    /// Passed Count.
    /// </summary>
    [JsonProperty("passed")]
    public int PassedCount;

    /// <summary>
    /// Failed Count.
    /// </summary>
    [JsonProperty("failed")]
    public int FailedCount;

    /// <summary>
    /// Miscast Count.
    /// </summary>
    [JsonProperty("miscast")]
    public int MiscastCount;

    /// <summary>
    /// Unevaluable Count.
    /// </summary>
    [JsonProperty("unevaluable")]
    public int UnevaluableCount;

    /// <summary>
    /// Empty Count.
    /// </summary>
    [JsonProperty("empty")]
    public int EmptyCount;

    /// <summary>
    /// Ignored Count.
    /// </summary>
    [JsonProperty("ignored")]
    public int IgnoredCount;
}

/// <summary>
/// DQ job run result id.
/// </summary>
public class ResultIdEventHubEntityModel
{
    /// <summary>
    /// BusinessDomainId
    /// </summary>
    [JsonProperty("businessDomainId")]
    public string BusinessDomainId { get; set; }

    /// <summary>
    /// DataProductId
    /// </summary>
    [JsonProperty("dataProductId")]
    public string DataProductId { get; set; }

    /// <summary>
    /// DataAssetId
    /// </summary>
    [JsonProperty("dataAssetId")]
    public string DataAssetId { get; set; }

    /// <summary>
    /// JobId
    /// </summary>
    [JsonProperty("jobId")]
    public string JobId { get; set; }
}

/// <summary>
/// JobRunState
/// </summary>
public enum JobRunState
{
    /// <summary>
    /// Failed
    /// </summary>
    Failed = 1,

    /// <summary>
    /// Succeeded
    /// </summary>
    Succeeded,

    /// <summary>
    /// InProgress
    /// </summary>
    InProgress,

    /// <summary>
    /// Cancelling
    /// </summary>
    Cancelling,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled,

    /// <summary>
    /// Queued
    /// </summary>
    Queued,

    /// <summary>
    /// Accepted
    /// </summary>
    Accepted,

    /// <summary>
    /// Rejected
    /// </summary>
    Rejected,

    /// <summary>
    /// Skipped
    /// </summary>
    Skipped,

    /// <summary>
    /// Deleting
    /// </summary>
    Deleting
}

/// <summary>
/// The data quality event model.
/// </summary>
public class DataQualitySourceEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// Job run region
    /// </summary>
    [JsonProperty("region")]
    public string Region { get; set; }

    /// <summary>
    /// Job result time.
    /// </summary>
    [JsonProperty("resultedAt")]
    public DateTime ResultedAt { get; set; }

    /// <summary>
    /// Job Status.
    /// </summary>
    [JsonProperty("jobStatus")]
    public string JobStatus { get; set; }

    /// <summary>
    /// DQ job result Id.
    /// </summary>
    [JsonProperty("id")]
    [JsonConverter(typeof(CustomTypeConverter<ResultIdEventHubEntityModel>))]
    public string ResultId { get; set; }

    /// <summary>
    /// Job results.
    /// </summary>
    [JsonProperty("facts")]
    [JsonConverter(typeof(CustomTypeConverter<Dictionary<string, JobResultValuesEventHubEntityModel>>))]
    public string Results { get; set; }

    /// <summary>
    /// Job result dimensions.
    /// </summary>
    [JsonProperty("dimensionMapping")]
    [JsonConverter(typeof(CustomTypeConverter<Dictionary<string, string>>))]
    public string Dimensions { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.DataQualityFact;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
        {
            new StructField("ResultId", DataTypes.String),
            new StructField("JobStatus", DataTypes.String),
            new StructField("ResultedAt", DataTypes.Timestamp),
            new StructField("Region", DataTypes.String),
            new StructField("Results", DataTypes.String),
            new StructField("Dimensions", DataTypes.String),
        }.ConcatArray(GetCommonSchemaFields()));
}

/// <summary>
/// The data quality score model.
/// </summary>
public class DataQualitySinkEventHubEntityModel : BaseEventHubEntityModel
{
    /// <summary>
    /// Row Id
    /// </summary>
    [JsonProperty("rowId")]
    public string RowId { get; set; }

    /// <summary>
    /// BusinessDomainId
    /// </summary>
    [JsonProperty("businessDomainId")]
    public string BusinessDomainId { get; set; }

    /// <summary>
    /// DataProductId.
    /// </summary>
    [JsonProperty("dataProductId")]
    public string DataProductId { get; set; }

    /// <summary>
    /// DataAssetId.
    /// </summary>
    [JsonProperty("dataAssetId")]
    public string DataAssetId { get; set; }

    /// <summary>
    /// JobId.
    /// </summary>
    [JsonProperty("jobId")]
    public string JobId { get; set; }

    /// <summary>
    /// QualityScore.
    /// </summary>
    [JsonProperty("qualityScore")]
    public double QualityScore { get; set; }

    /// <summary>
    /// Score calculation time.
    /// </summary>
    [JsonProperty("resultedAt")]
    public DateTime ResultedAt { get; set; }

    /// <inheritdoc/>
    public override PayloadKind GetPayloadKind() => PayloadKind.DataQualityScore;

    /// <inheritdoc/>
    public override StructType GetSchemaDefinition() => new(new StructField[]
        {
            new StructField("RowId", DataTypes.String),
            new StructField("AccountId", DataTypes.String),
            new StructField("BusinessDomainId", DataTypes.String),
            new StructField("DataProductId", DataTypes.String),
            new StructField("DataAssetId", DataTypes.String),
            new StructField("JobId", DataTypes.String),
            new StructField("QualityScore", DataTypes.Double),
            new StructField("ResultedAt", DataTypes.Timestamp),
        });
}
