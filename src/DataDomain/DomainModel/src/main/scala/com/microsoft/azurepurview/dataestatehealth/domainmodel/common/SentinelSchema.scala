package com.microsoft.azurepurview.dataestatehealth.domainmodel.common
import org.apache.spark.sql.types.{ IntegerType, StringType, StructField, StructType, TimestampType, LongType}

class SentinelSchema {
  val sentinelSchema: StructType = StructType(
    Array(
      StructField("id", StringType, nullable = false),
      StructField("WorkerJobExecutionId", StringType, nullable = false),
      StructField("DEHScope", StringType, nullable = false),
      StructField("accountId", StringType, nullable = false),
      StructField("ADLSPath", StringType, nullable = true),
      StructField("Entity", StringType, nullable = true),
      StructField("OperatedDFCount", LongType, nullable = true),
      StructField("Status", StringType, nullable = true),
      StructField("ModifiedDateTime", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = true),
      StructField("EventProcessingTimestamp", StringType, nullable = true),
      StructField("ExceptionStackTrace", StringType, nullable = true)
    )
  )
}