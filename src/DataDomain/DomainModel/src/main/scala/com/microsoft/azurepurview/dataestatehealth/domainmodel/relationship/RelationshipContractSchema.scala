package com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship
import org.apache.spark.sql.types._
class RelationshipContractSchema {
  val relationshipContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("Type", StringType),
        StructField("SourceType", StringType),
        StructField("SourceId", StringType),
        StructField("TargetType", StringType),
        StructField("TargetId", StringType),
        StructField("Properties", StringType)
      ))),
      StructField("after", StructType(Seq(
        StructField("Type", StringType),
        StructField("SourceType", StringType),
        StructField("SourceId", StringType),
        StructField("TargetType", StringType),
        StructField("TargetId", StringType),
        StructField("Properties", StringType)
      ))),
      StructField("related", StringType)
    ))),
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
