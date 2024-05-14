package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDataAssetSchema {
  val dimDataAssetSchema: StructType = StructType(
    Array(
      StructField("DataAssetId", StringType, nullable = false),
      StructField("DataAssetSourceId", StringType, nullable = false),
      StructField("DataAssetDisplayName", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = false),
      StructField("ModifiedDatetime", TimestampType, nullable = false),
      StructField("EffectiveDateTime", TimestampType, nullable = false),
      StructField("ExpiredDateTime", TimestampType, nullable = true),
      StructField("CurrentIndicator",IntegerType,  nullable = false)
    )
  )
}