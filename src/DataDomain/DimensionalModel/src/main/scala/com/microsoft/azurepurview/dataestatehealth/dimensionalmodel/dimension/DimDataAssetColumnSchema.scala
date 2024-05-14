package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDataAssetColumnSchema {
  val dimDataAssetColumnSchema: StructType = StructType(
    Array(
      StructField("DataAssetColumnId", StringType, nullable = false),
      StructField("DataAssetColumnSourceId", StringType, nullable = false),
      StructField("DataAssetColumnDisplayName", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = false),
      StructField("ModifiedDatetime", TimestampType, nullable = false),
      StructField("EffectiveDateTime", TimestampType, nullable = false),
      StructField("ExpiredDateTime", TimestampType, nullable = true),
      StructField("CurrentIndicator",IntegerType,  nullable = false)
    )
  )
}