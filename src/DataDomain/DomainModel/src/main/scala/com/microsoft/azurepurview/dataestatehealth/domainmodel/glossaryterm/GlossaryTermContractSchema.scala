package com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm
import org.apache.spark.sql.types._
class GlossaryTermContractSchema {
  val glossarytermContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        ))),
        StructField("description", StringType),
        StructField("contacts", StructType(Seq(
          StructField("owner", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          )))),
          StructField("expert", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          )))),
          StructField("databaseAdmin", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          ))))
        ))),
        StructField("domain", StringType),
        StructField("status", StringType),
        StructField("parentId", StringType),
        StructField("acronyms", ArrayType(StringType)),
        StructField("resources", ArrayType(StructType(Seq(
          StructField("name", StringType),
          StructField("url", StringType)
        ))))
      ))),
      StructField("after", StructType(Seq(
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        ))),
        StructField("description", StringType),
        StructField("contacts", StructType(Seq(
          StructField("owner", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          )))),
          StructField("expert", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          )))),
          StructField("databaseAdmin", ArrayType(StructType(Seq(
            StructField("id", StringType),
            StructField("description", StringType)
          ))))
        ))),
        StructField("domain", StringType),
        StructField("status", StringType),
        StructField("parentId", StringType),
        StructField("acronyms", ArrayType(StringType)),
        StructField("resources", ArrayType(StructType(Seq(
          StructField("name", StringType),
          StructField("url", StringType)
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
