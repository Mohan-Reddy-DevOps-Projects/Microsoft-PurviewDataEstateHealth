package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class DataQualityJobExecutionSchema {
  val dataQualityJobExecutionSchema: StructType = StructType(
    Array(
      StructField("JobExecutionId", StringType, nullable = false),
      StructField("JobExecutionStatusDisplayName", StringType, nullable = false),
      StructField("JobType", StringType, nullable = true),
      StructField("ScanTypeDisplayName", StringType, nullable = true),
      StructField("JobCreationDatetime", TimestampType, nullable = true),
      StructField("ExecutionStartDatetime", TimestampType, nullable = true),
      StructField("ExecutionEndDatetime", TimestampType, nullable = true)
    )
  )
}