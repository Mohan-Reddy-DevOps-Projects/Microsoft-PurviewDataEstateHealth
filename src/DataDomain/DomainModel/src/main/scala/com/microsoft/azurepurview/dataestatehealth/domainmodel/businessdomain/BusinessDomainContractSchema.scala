package com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain
import org.apache.spark.sql.types._
class BusinessDomainContractSchema {
  val businessDomainContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        ))),
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("description", StringType),
        StructField("parentId", StringType),
        StructField("status", StringType),
        StructField("type", StringType),
        StructField("thumbnail", StructType(Seq(
          StructField("color", StringType)
        ))),
        StructField("domains", ArrayType(StructType(Seq(
          StructField("name", StringType),
          StructField("friendlyName", StringType),
          StructField("relatedCollections", ArrayType(StructType(Seq(
            StructField("name", StringType),
            StructField("friendlyName", StringType),
            StructField("parentCollection", StructType(Seq(
              StructField("refName", StringType),
              StructField("type", StringType)
            )))
          ))))
        ))))
      ))),
      StructField("after", StructType(Seq(
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        ))),
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("description", StringType),
        StructField("parentId", StringType),
        StructField("status", StringType),
        StructField("type", StringType),
        StructField("thumbnail", StructType(Seq(
          StructField("color", StringType)
        ))),
        StructField("domains", ArrayType(StructType(Seq(
          StructField("name", StringType),
          StructField("friendlyName", StringType),
          StructField("relatedCollections", ArrayType(StructType(Seq(
            StructField("name", StringType),
            StructField("friendlyName", StringType),
            StructField("parentCollection", StructType(Seq(
              StructField("refName", StringType),
              StructField("type", StringType)
            )))
          ))))
        ))))
      )))
    ))),
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
