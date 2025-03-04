package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import org.apache.spark.sql.types._

class HealthActionContractSchema {
  val healthActionContractSchema: StructType = StructType(Seq(
    StructField("JObject", StructType(Seq(
      StructField("category", StringType),
      StructField("severity", StringType),
      StructField("findingId", StringType),
      StructField("findingName", StringType),
      StructField("reason", StringType, true),
      StructField("recommendation", StringType, true),
      StructField("findingType", StringType),
      StructField("findingSubType", StringType),
      StructField("targetEntityType", StringType),
      StructField("targetEntityId", StringType),
      StructField("assignedTo", ArrayType(StringType)),
      StructField("domainId", StringType),
      StructField("extraProperties", StructType(Seq(
        StructField("type", StringType),
        StructField("data", StructType(Seq(
          StructField("jobId", StringType)
        )))
      ))),
      StructField("status", StringType),
      StructField("id", StringType),
      StructField("systemData", StructType(Seq(
        StructField("lastModifiedAt", StringType),
        StructField("lastModifiedBy", StringType),
        StructField("createdAt", StringType),
        StructField("createdBy", StringType),
        StructField("lastHintAt", StringType),
        StructField("hintCount", IntegerType)
      )))
    ))),
    StructField("TenantId", StringType),
    StructField("AccountId", StringType),
    StructField("id", StringType),
    StructField("_rid", StringType),
    StructField("_etag", StringType),
    StructField("_ts", LongType)
  ))
}
