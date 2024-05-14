package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class DataProductOwnerSchema {
  val dataProductOwnerSchema: StructType = StructType(
    Array(
      StructField("DataProductId", StringType, nullable = false),
      StructField("DataProductOwnerId", StringType, nullable = false),
      StructField("DataProductOwnerDescription", StringType, nullable = true)
    )
  )
}
