package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import org.apache.spark.sql.types.{ArrayType, IntegerType, LongType, StringType, StructField, StructType}

class KeyResultContractSchema {
  val keyResultContractSchema : StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("id", StringType),
        StructField("definition", StringType),
        StructField("domainId", StringType),
        StructField("progress", StringType),
        StructField("goal", StringType),
        StructField("max", StringType),
        StructField("status", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
        )))
      ))),
      StructField("after", StructType(Seq(
        StructField("id", StringType),
        StructField("definition", StringType),
        StructField("domainId", StringType),
        StructField("progress", StringType),
        StructField("goal", StringType),
        StructField("max", StringType),
        StructField("status", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
        )))
      ))),
      StructField("related", StructType(Seq(
        StructField("dataAssetId", ArrayType(StringType))
      )))))),
    StructField("eventSource", StringType),
    StructField("payloadKind", StringType),
    StructField("operationType", StringType),
    StructField("preciseTimestamp", StringType),
    StructField("tenantId", StringType),
    StructField("accountId", StringType),
    StructField("changedBy", StringType),
    StructField("eventId", StringType),
    StructField("correlationId", StringType),
    StructField("EventProcessedUtcTime", StringType),
    StructField("PartitionId", IntegerType),
    StructField("EventEnqueuedUtcTime", StringType),
    StructField("id", StringType),
    StructField("_rid", StringType),
    StructField("_etag", StringType),
    StructField("_ts", LongType)
  ))
}
