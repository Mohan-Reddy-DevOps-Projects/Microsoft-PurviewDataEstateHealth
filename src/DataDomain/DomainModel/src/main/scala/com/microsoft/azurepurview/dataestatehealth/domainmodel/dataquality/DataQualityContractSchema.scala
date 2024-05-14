package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import org.apache.spark.sql.types._
class DataQualityContractSchema {
  val dataQualityContractSchema: StructType = StructType(Seq(
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
      StructField("ruleMetadata", MapType(StringType, StructType(Seq(
        StructField("fields", ArrayType(StringType)),
        StructField("dimension", StringType)
      )))),
      StructField("domainmodel", StructType(Seq(
        StructField("eventSource", StringType),
        StructField("eventCorrelationId", StringType),
        StructField("payloadKind", StringType),
        StructField("operationType", StringType),
        StructField("payload", StructType(Seq(
          StructField("accountId", StringType),
          StructField("businessDomainId", StringType),
          StructField("dataProductId", StringType),
          StructField("dataAssetId", StringType),
          StructField("jobId", StringType),
          StructField("jobType", StringType),
          StructField("dataQualityFact", ArrayType(StructType(Seq(
            StructField("businessDomainId", StringType),
            StructField("dataProductId", StringType),
            StructField("dataAssetId", StringType),
            StructField("jobExecutionId", StringType),
            StructField("ruleId", StringType),
            StructField("ruleType", StringType),
            StructField("column", StringType),
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
    StructField("EventProcessedUtcTime", StringType),
    StructField("PartitionId", IntegerType),
    StructField("EventEnqueuedUtcTime", StringType),
    StructField("id", StringType),
    StructField("_rid", StringType),
    StructField("_etag", StringType),
    StructField("_ts", LongType)
  ))
}
