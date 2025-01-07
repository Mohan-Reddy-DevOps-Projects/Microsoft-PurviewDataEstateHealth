package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType, TimestampType}

class CDEGlossaryTermAssignmentSchema {
  val cdeGlossaryTermAssignmentSchema: StructType = StructType(
    Array(
      StructField("CDEId", StringType, nullable = false),
      StructField("GlossaryTermId", StringType, nullable = false),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}
