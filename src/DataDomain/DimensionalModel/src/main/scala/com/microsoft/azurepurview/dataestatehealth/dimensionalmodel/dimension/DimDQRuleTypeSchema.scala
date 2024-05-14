package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDQRuleTypeSchema {
  val dimDQRuleTypeSchema: StructType = StructType(
    Array(
      StructField("DQRuleTypeId", LongType, nullable = false),
      StructField("DQRuleTypeSourceId", StringType, nullable = false),
      StructField("DQRuleTypeDisplayName", StringType, nullable = false),
      StructField("QualityDimension", StringType, nullable = true),
      StructField("QualityDimensionCustomIndicator", StringType, nullable = true)
    )
  )
}

