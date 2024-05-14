package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}

class GlossaryTermSchema {
  val glossaryTermSchema: StructType = StructType(
    Array(
      StructField("GlossaryTermId", StringType, nullable = false),
      StructField("ParentGlossaryTermId", StringType, nullable = true),
      StructField("GlossaryTermDisplayName", StringType, nullable = false),
      StructField("GlossaryDescription", StringType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("IsLeaf", IntegerType, nullable = true),
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
