package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType, TimestampType}

class CDEColumnAssignmentSchema {
  val cdeColumnAssignmentSchema: StructType = StructType(
    Array(
      StructField("CDEId", StringType, nullable = false),
      StructField("ColumnId", StringType, nullable = false),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
