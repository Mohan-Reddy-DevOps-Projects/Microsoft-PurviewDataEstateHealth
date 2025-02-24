package com.microsoft.azurepurview.dataestatehealth.domainmodel.objective

import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType, TimestampType}

class DataProductOKRAssignmentSchema {
  val dataProductOKRAssignmentSchema: StructType = StructType(
    Array(
      StructField("ObjectiveId", StringType, nullable = false),
      StructField("DataProductId", StringType, nullable = false),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
