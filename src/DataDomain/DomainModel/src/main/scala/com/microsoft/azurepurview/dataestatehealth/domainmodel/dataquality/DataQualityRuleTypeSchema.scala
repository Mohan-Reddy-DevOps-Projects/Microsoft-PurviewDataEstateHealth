package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import org.apache.spark.sql.types.{IntegerType, StringType, StructField, StructType}
class DataQualityRuleTypeSchema {
  val dataQualityRuleTypeSchema: StructType = StructType(
    Array(
      StructField("RuleTypeId", StringType, nullable = false),
      StructField("RuleTypeDisplayName", StringType, nullable = false),
      StructField("RuleTypeDesc", StringType, nullable = true),
      StructField("DimensionDisplayName", StringType, nullable = true)
    )
  )
}