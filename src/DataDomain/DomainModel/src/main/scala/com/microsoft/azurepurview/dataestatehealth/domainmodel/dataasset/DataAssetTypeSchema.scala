package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetTypeSchema {
  val dataAssetTypeSchema: StructType = StructType(
    Array(
      StructField("DataAssetTypeId", StringType, nullable = false),
      StructField("DataAssetTypeDisplayName", StringType, nullable = false)
    )
  )
}
