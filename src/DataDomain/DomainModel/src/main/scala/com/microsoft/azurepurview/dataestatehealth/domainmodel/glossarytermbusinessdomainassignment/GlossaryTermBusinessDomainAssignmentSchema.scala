package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossarytermbusinessdomainassignment
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class GlossaryTermBusinessDomainAssignmentSchema {
  val glossaryTermBusinessDomainAssignmentSchema: StructType = StructType(
    Array(
      StructField("GlossaryTermID", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("AssignedByUserId", StringType, nullable = true),
      StructField("AssignmentDatetime", TimestampType, nullable = true),
      StructField("GlossaryTermStatus", StringType, nullable = false),
      StructField("ActiveFlag", IntegerType, nullable = false),
      StructField("ActiveFlagLastModifiedDatetime", TimestampType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
