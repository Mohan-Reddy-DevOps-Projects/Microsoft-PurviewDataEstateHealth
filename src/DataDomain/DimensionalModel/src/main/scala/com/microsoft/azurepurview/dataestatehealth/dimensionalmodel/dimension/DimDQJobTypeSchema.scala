package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension

import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class DimDQJobTypeSchema {
  val dimDQJobTypeSchema: StructType = StructType(
    Array(
      StructField("JobTypeId", LongType, nullable = false),
      StructField("JobTypeDisplayName", StringType, nullable = false)
    )
  )
}
