package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import org.apache.spark.sql.types.{DecimalType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class DataQualityRuleColumnExecutionSchema {
  val dataQualityRuleColumnExecutionSchema: StructType = StructType(
    Array(
      StructField("JobExecutionId", StringType, nullable = false),
      StructField("RuleId", StringType, nullable = false),
      StructField("DataAssetId", StringType, nullable = false),
      StructField("ColumnId", StringType, nullable = true),
      StructField("ColumnResultScore", DecimalType (18, 10), nullable = true),
      StructField("RowPassCount", IntegerType, nullable = true),
      StructField("RowFailCount", IntegerType, nullable = true),
      StructField("RowMiscastCount", IntegerType, nullable = true),
      StructField("RowEmptyCount", IntegerType, nullable = true),
      StructField("RowIgnoredCount", IntegerType, nullable = true)
    )
  )
}