package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproductupdatefrequency

import org.apache.spark.sql.types.{IntegerType, StringType, StructField, StructType}

class DataProductUpdateFrequencySchema {
  val dataProductUpdateFrequencySchema: StructType = StructType(
    Array(
      StructField("UpdateFrequencyID", StringType, nullable = false),
      StructField("UpdateFrequencyDisplayName", StringType, nullable = false),
      StructField("SortOrder", IntegerType, nullable = true)
    )
  )
}
