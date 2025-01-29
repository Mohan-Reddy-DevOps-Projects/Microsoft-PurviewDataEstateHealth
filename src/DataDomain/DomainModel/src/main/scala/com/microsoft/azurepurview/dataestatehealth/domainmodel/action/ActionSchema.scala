package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType, TimestampType}

class ActionSchema {
  val actionSchema: StructType = StructType(
    Array(
      StructField("ActionId", StringType, nullable = false),
      StructField("AccountId", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("Category", StringType, nullable = false),
      StructField("Severity", StringType, nullable = true),
      StructField("FindingId", StringType, nullable = true),
      StructField("FindingName", StringType, nullable = true),
      StructField("Reason", StringType, nullable = true),
      StructField("Recommendation", StringType, nullable = true),
      StructField("FindingType", StringType, nullable = false),
      StructField("FindingSubType", StringType, nullable = false),
      StructField("TargetEntityType", StringType, nullable = false),
      StructField("Type", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true)
    )
  )
}
