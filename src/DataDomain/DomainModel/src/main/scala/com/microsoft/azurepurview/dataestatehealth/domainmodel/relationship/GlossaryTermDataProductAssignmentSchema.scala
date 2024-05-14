package com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class GlossaryTermDataProductAssignmentSchema {
  val glossaryTermDataProductAssignmentSchema: StructType = StructType(
    Array(
      StructField("GlossaryTermID", StringType, nullable = false),
      StructField("DataProductId", StringType, nullable = false),
      StructField("ActiveFlag", IntegerType, nullable = false),
      StructField("ActiveFlagLastModifiedDatetime", TimestampType, nullable = false),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
