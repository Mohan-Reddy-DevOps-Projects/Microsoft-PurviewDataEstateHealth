package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetTypeDataTypeSchema {
  val dataAssetTypeDataTypeSchema: StructType = StructType(
    Array(
      StructField("DataTypeId", StringType, nullable = false),
      StructField("DataAssetTypeId", StringType, nullable = false),
      StructField("DataTypeDisplayName", StringType, nullable = false)
    )
  )
}