package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproducttype

import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class DataProductTypeSchema {
  val dataProductTypeSchema: StructType = StructType(
    Array(
      StructField("DataProductTypeID", StringType, nullable = false),
      StructField("DataProductTypeDisplayName", StringType, nullable = false),
      StructField("DataProductTypeDescription", StringType, nullable = true)
    )
  )
}
