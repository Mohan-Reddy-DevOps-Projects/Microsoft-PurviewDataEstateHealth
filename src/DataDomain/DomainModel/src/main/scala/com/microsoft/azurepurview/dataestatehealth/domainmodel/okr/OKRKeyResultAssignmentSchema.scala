package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType, TimestampType}

class OKRKeyResultAssignmentSchema {
  val okrKeyResultAssignmentSchema: StructType = StructType(
    Array(
      StructField("OKRId", StringType, nullable = false),
      StructField("KeyResultId", StringType, nullable = false),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
