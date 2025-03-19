package com.microsoft.azurepurview.dataestatehealth.domainmodel.objective

import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class KeyResultSchema {
  val keyResultSchema: StructType = StructType(
    Array(
      StructField("KeyResultId", StringType, nullable = false),
      StructField("ObjectiveId", StringType, nullable = false),
      StructField("KeyResultDisplayName", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("Progress", IntegerType, nullable = false),
      StructField("Goal", IntegerType, nullable = true),
      StructField("Maximum", IntegerType, nullable = true),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false)
    )
  )
}
