package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDQScanProfileSchema {
  val dimDQScanProfileSchema: StructType = StructType(
    Array(
      StructField("DQScanProfileId", LongType, nullable = false),
      StructField("RuleOriginDisplayName", StringType, nullable = false),
      StructField("RuleAppliedOn", StringType, nullable = false)
    )
  )
}
