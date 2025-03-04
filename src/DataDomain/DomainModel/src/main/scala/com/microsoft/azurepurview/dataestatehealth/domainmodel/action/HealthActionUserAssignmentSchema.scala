package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import org.apache.spark.sql.types.{StringType, StructField, StructType}

class HealthActionUserAssignmentSchema {
  val healthActionUserAssignmentSchema: StructType = StructType(
    Array(
      StructField("ActionId", StringType, nullable = true),
      StructField("AssignedToUserId", StringType, nullable = true)
    )
  )
}
