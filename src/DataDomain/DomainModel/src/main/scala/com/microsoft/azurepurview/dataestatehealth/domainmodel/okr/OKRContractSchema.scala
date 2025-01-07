package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr
import org.apache.spark.sql.types._

class OKRContractSchema {
  val okrContractSchema : StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("id", StringType),
        StructField("definition", StringType),
        StructField("domain", StringType),
        StructField("targetDate", StringType),
        StructField("contacts", StructType(Seq(
          StructField("owner", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          )))),
        ))),
        StructField("status", StringType),
        StructField("additionalProperties", StructType(Seq(
          StructField("OverallStatus", StringType),
          StructField("OverallProgress", IntegerType),
          StructField("OverallGoal", IntegerType),
          StructField("OverallMax", IntegerType),
          StructField("KeyResultsCount", IntegerType),
        ))),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        )))
      ))),
      StructField("after", StructType(Seq(
        StructField("id", StringType),
        StructField("definition", StringType),
        StructField("domain", StringType),
        StructField("targetDate", StringType),
        StructField("contacts", StructType(Seq(
          StructField("owner", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          )))),
        ))),
        StructField("status", StringType),
        StructField("additionalProperties", StructType(Seq(
          StructField("OverallStatus", StringType),
          StructField("OverallProgress", IntegerType),
          StructField("OverallGoal", IntegerType),
          StructField("OverallMax", IntegerType),
          StructField("KeyResultsCount", IntegerType),
        ))),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
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
