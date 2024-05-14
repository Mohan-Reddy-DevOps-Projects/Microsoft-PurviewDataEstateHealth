package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductstatus

import org.apache.spark.sql.types.{IntegerType, StringType, StructField, StructType}

class DataProductStatusSchema {
  val dataProductStatusSchema: StructType = StructType(
    Array(
      StructField("DataProductStatusID", StringType, nullable = false),
      StructField("DataProductStatusDisplayName", StringType, nullable = false)
    )
  )
}
