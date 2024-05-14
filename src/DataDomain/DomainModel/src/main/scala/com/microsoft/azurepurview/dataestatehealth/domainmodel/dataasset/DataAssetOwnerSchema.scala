package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetOwnerSchema {
  val dataAssetOwnerSchema: StructType = StructType(
    Array(
      StructField("DataAssetOwnerId", StringType, nullable = false),
      StructField("DataAssetOwner", StringType, nullable = false)
    )
  )
}
