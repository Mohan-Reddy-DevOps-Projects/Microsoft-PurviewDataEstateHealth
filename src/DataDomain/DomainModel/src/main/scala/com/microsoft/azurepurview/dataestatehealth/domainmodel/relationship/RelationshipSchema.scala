package com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship
import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class RelationshipSchema {
  val relationshipSchema: StructType = StructType(
    Array(
      StructField("AccountId", StringType, nullable = false),
      StructField("Type", StringType, nullable = false),
      StructField("SourceType", StringType, nullable = false),
      StructField("SourceId", StringType, nullable = false),
      StructField("TargetType", StringType, nullable = false),
      StructField("TargetId", StringType, nullable = false),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = false),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)

    )
  )
}
