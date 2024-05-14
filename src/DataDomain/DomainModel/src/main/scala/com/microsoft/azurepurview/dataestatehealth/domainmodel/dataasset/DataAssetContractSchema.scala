package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types._
class DataAssetContractSchema {
  val dataAssetContractSchema: StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType)
        ))),
        StructField("description", StringType),
        StructField("domain", StringType),
        StructField("source", StructType(Seq(
          StructField("type", StringType),
          StructField("assetId", StringType),
          StructField("assetType", StringType),
          StructField("fqn", StringType),
          StructField("accountName", StringType),
          StructField("lastRefreshedAt", StringType),
          StructField("lastRefreshedBy", StringType)
        ))),
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
        StructField("classifications", ArrayType(StringType)),
        StructField("sensitivityLabel", StringType),
        StructField("type", StringType),
        StructField("schema", ArrayType(StructType(Seq(
          StructField("name", StringType),
          StructField("description", StringType),
          StructField("classifications", ArrayType(StringType)),
          StructField("type", StringType)
        )))),
        StructField("lineage", MapType(StringType, StringType)),
        StructField("typeProperties", StructType(Seq(
          StructField("format", StringType),
          StructField("serverEndpoint", StringType),
          StructField("databaseName", StringType),
          StructField("schemaName", StringType),
          StructField("tableName", StringType)
        )))
      ))),
      StructField("after", StructType(Seq(
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType)
        ))),
        StructField("description", StringType),
        StructField("domain", StringType),
        StructField("source", StructType(Seq(
          StructField("type", StringType),
          StructField("assetId", StringType),
          StructField("assetType", StringType),
          StructField("fqn", StringType),
          StructField("accountName", StringType),
          StructField("lastRefreshedAt", StringType),
          StructField("lastRefreshedBy", StringType)
        ))),
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
        StructField("classifications", ArrayType(StringType)),
        StructField("sensitivityLabel", StringType),
        StructField("type", StringType),
        StructField("schema", ArrayType(StructType(Seq(
          StructField("name", StringType),
          StructField("description", StringType),
          StructField("classifications", ArrayType(StringType)),
          StructField("type", StringType)
        )))),
        StructField("lineage", MapType(StringType, StringType)),
        StructField("typeProperties", StructType(Seq(
          StructField("format", StringType),
          StructField("serverEndpoint", StringType),
          StructField("databaseName", StringType),
          StructField("schemaName", StringType),
          StructField("tableName", StringType)
        )))
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
