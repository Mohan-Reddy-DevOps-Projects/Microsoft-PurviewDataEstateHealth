package com.microsoft.azurepurview.dataestatehealth.controls.model

import org.apache.spark.sql.types._

/**
 * Schema definition for the Data Quality Contract.
 * This represents the structure used for quality assessment results.
 */
class ControlSummarySchema {
  val controlSummarySchema: StructType = StructType(Seq(
    StructField("payloadDetails", StructType(Seq(
      StructField("id", StructType(Seq(
        StructField("businessDomainId", StringType),
        StructField("dataProductId", StringType),
        StructField("dataAssetId", StringType),
        StructField("jobId", StringType)
      ))),
      StructField("jobStatus", StringType),
      StructField("resultedAt", StringType),
      StructField("region", StringType),
      StructField("facts", MapType(StringType, StructType(Seq(
        StructField("passed", IntegerType),
        StructField("failed", IntegerType),
        StructField("miscast", IntegerType),
        StructField("unevaluable", IntegerType),
        StructField("empty", IntegerType),
        StructField("ignored", IntegerType)
      )))),
      StructField("dimensionMapping", MapType(StringType, StringType)),
      StructField("domainmodel", StructType(Seq(
        StructField("eventSource", StringType),
        StructField("eventCorrelationId", StringType),
        StructField("payloadKind", StringType),
        StructField("operationType", StringType),
        StructField("payload", StructType(Seq(
          StructField("accountId", StringType),
          StructField("tenantId", StringType),
          StructField("businessDomainId", StringType),
          StructField("dataProductId", StringType),
          StructField("dataAssetId", StringType),
          StructField("jobId", StringType),
          StructField("puDetail", StringType),
          StructField("jobType", StringType),
          StructField("dataQualityFact", ArrayType(StructType(Seq(
            StructField("businessDomainId", StringType),
            StructField("dataProductId", StringType),
            StructField("dataAssetId", StringType),
            StructField("jobExecutionId", StringType),
            StructField("ruleId", StringType),
            StructField("ruleType", StringType),
            StructField("dimension", StringType),
            StructField("result", DoubleType),
            StructField("passed", IntegerType),
            StructField("failed", IntegerType),
            StructField("miscast", IntegerType),
            StructField("empty", IntegerType),
            StructField("ignored", IntegerType),
            StructField("isAppliedOn", StringType),
            StructField("ruleOrigin", StringType)
          )))),
          StructField("systemData", StructType(Seq(
            StructField("createdAt", StringType),
            StructField("resultedAt", StringType)
          )))
        )))
      )))
    ))),
    StructField("accountId", StringType),
    StructField("eventId", StringType),
    StructField("correlationId", StringType),
    StructField("preciseTimestamp", StringType),
    StructField("payloadKind", StringType),
    StructField("operationType", StringType),
    StructField("payloadType", StringType),
    StructField("puDetail", StringType),
    StructField("EventProcessedUtcTime", StringType),
    StructField("PartitionId", IntegerType),
    StructField("EventEnqueuedUtcTime", StringType),
    StructField("id", StringType),
    StructField("_rid", StringType),
    StructField("_etag", StringType),
    StructField("_ts", LongType)
  ))
} 