package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DimBusinessDomainSchema {
  val dimBusinessDomainSchema: StructType = StructType(
    Array(
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("BusinessDomainSourceId", StringType, nullable = false),
      StructField("BusinessDomainDisplayName", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = false),
      StructField("ModifiedDatetime", TimestampType, nullable = false),
      StructField("EffectiveDateTime", TimestampType, nullable = false),
      StructField("ExpiredDateTime", TimestampType, nullable = true),
      StructField("CurrentIndicator",IntegerType,  nullable = false)
    )
  )
}