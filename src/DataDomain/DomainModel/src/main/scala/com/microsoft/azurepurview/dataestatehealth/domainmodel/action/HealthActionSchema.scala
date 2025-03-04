package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType, TimestampType}

class HealthActionSchema {
  val healthActionSchema: StructType = StructType(
    Array(
      StructField("ActionId", StringType, nullable = false),
      StructField("FindingDisplayName", StringType, nullable = true),
      StructField("FindingTypeId", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("Category", StringType, nullable = false),
      StructField("Severity", StringType, nullable = true),
      StructField("Reason", StringType, nullable = true),
      StructField("Recommendation", StringType, nullable = true),
      StructField("TargetEntityType", StringType, nullable = false),
      StructField("TargetEntityId", StringType, nullable = true),
      StructField("Type", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true)
    )
  )
}
