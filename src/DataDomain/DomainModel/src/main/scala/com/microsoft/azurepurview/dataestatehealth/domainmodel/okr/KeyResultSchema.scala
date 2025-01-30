package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class KeyResultSchema {
  val keyResultSchema: StructType = StructType(
    Array(
      StructField("KeyResultId", StringType, nullable = false),
      StructField("KeyResultDefintion", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("Progress", IntegerType, nullable = false),
      StructField("Goal", IntegerType, nullable = true),
      StructField("Max", IntegerType, nullable = true),
      StructField("OKRId", StringType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = true)
    )
  )
}
