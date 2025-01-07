package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class CDESchema {
  val cdeSchema: StructType = StructType(
    Array(
      StructField("Name", StringType, nullable = false),
      StructField("DataType", StringType, nullable = false),
      StructField("Status", StringType, nullable = true),
      StructField("Description", StringType, nullable = true),
      StructField("CDEId", StringType, nullable = true),
      StructField("AccountId", StringType, nullable = true),
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
