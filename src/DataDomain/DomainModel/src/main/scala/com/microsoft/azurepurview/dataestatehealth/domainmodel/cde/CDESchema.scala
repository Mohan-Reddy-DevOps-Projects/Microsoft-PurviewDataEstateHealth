package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class CDESchema {
  val cdeSchema: StructType = StructType(
    Array(
      StructField("CriticalDataElementId", StringType, nullable = true),
      StructField("CriticalDataElementDisplayName", StringType, nullable = false),
      StructField("CriticalDataElementDescription", StringType, nullable = true),
      StructField("CriticalDataElementStatus", StringType, nullable = true),
      StructField("ExpectedDataType", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false)
    )
  )
}
