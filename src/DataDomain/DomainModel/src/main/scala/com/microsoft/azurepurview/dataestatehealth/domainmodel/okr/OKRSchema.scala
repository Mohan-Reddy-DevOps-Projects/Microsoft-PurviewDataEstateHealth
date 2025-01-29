package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class OKRSchema {
  val okrSchema: StructType = StructType(
    Array(
      StructField("OKRId", StringType, nullable = false),
      StructField("OKRDefintion", StringType, nullable = false),
      StructField("Status", StringType, nullable = false),
      StructField("TargetDate", StringType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
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
