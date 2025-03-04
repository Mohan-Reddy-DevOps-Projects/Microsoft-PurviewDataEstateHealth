package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import org.apache.spark.sql.types.{StringType, StructField, StructType}

class HealthActionFindingSubTypeSchema {
  val healthActionFindingSubTypeSchema: StructType = StructType(
    Array(
      StructField("FindingSubTypeId", StringType, nullable = false),
      StructField("FindingSubTypeDisplayName", StringType, nullable = false),
      StructField("FindingTypeId", StringType, nullable = false)
    )
  )
}
