package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import org.apache.spark.sql.types._

class CDEContractSchema {
  val cdeContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("name", StringType),
        StructField("dataType", StringType),
        StructField("status", StringType),
        StructField("contacts", StructType(Seq(
          StructField("owner", ArrayType(StructType(Seq(
            StructField("id", StringType)
          )))),
        ))),
        StructField("description", StringType),
        StructField("domain", StringType),
        StructField("id", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType)
        )))
      ))),
      StructField("after", StructType(Seq(
        StructField("name", StringType),
        StructField("dataType", StringType),
        StructField("status", StringType),
        StructField("contacts", StructType(Seq(
          StructField("owner", ArrayType(StructType(Seq(
            StructField("id", StringType)
          )))),
        ))),
        StructField("description", StringType),
        StructField("domain", StringType),
        StructField("id", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType)
        )))
      ))),
      StructField("related", StructType(Seq(
        StructField("dataAssetId", ArrayType(StringType))
      )))))),
    StructField("eventSource", StringType),
    StructField("payloadKind", StringType),
    StructField("operationType", StringType),
    StructField("preciseTimestamp", StringType),
    StructField("tenantId", StringType),
    StructField("accountId", StringType),
    StructField("changedBy", StringType),
    StructField("eventId", StringType),
    StructField("correlationId", StringType),
    StructField("EventProcessedUtcTime", StringType),
    StructField("PartitionId", IntegerType),
    StructField("EventEnqueuedUtcTime", StringType),
    StructField("id", StringType),
    StructField("_rid", StringType),
    StructField("_etag", StringType),
    StructField("_ts", LongType)
  ))
}
