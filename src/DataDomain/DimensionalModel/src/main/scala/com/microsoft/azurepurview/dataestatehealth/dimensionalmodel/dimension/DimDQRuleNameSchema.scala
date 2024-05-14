package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDQRuleNameSchema {
  val dimDQRuleNameSchema: StructType = StructType(
    Array(
      StructField("DQRuleId", LongType, nullable = false),
      StructField("DQRuleNameId", StringType, nullable = false),
      StructField("DQRuleNameSourceId", StringType, nullable = false),
      StructField("DQRuleNameDisplayName", StringType, nullable = true)
    )
  )
}