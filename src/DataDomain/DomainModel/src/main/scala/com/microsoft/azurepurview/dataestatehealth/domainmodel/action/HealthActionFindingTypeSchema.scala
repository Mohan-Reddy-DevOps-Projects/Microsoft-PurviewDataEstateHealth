package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import org.apache.spark.sql.types.{StringType, StructField, StructType}

class HealthActionFindingTypeSchema {
  val healthActionFindingTypeSchema: StructType = StructType(
    Array(
      StructField("FindingTypeId", StringType, nullable = false),
      StructField("FindingTypeDisplayName", StringType, nullable = false)
    )
  )
}
