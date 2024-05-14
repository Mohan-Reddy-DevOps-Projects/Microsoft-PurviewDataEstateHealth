package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimDataProductSchema {
  val dimDataProductSchemaSchema: StructType = StructType(
    Array(
      StructField("DataProductId", StringType, nullable = false),
      StructField("DataProductSourceId", StringType, nullable = false),
      StructField("DataProductDisplayName", StringType, nullable = false),
      StructField("DataProductStatus", StringType, nullable = true),
      StructField("CreatedDatetime", TimestampType, nullable = false),
      StructField("ModifiedDatetime", TimestampType, nullable = false),
      StructField("IsActive",BooleanType,  nullable = false)
    )
  )
}