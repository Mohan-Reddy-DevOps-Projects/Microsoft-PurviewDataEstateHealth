package com.microsoft.azurepurview.dataestatehealth.domainmodel.objective

import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class ObjectiveSchema {
  val objectiveSchema: StructType = StructType(
    Array(
      StructField("ObjectiveId", StringType, nullable = false),
      StructField("ObjectiveDisplayName", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("TargetDate", TimestampType, nullable = true),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = true),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
