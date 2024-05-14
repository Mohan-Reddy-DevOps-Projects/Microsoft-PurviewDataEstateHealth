package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import org.apache.spark.sql.types._
class DataProductContractSchema {
  val dataProductContractSchema : StructType = StructType(Seq(
    StructField("payload", StructType(Seq(
      StructField("before", StructType(Seq(
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("domain", StringType),
        StructField("type", StringType),
        StructField("description", StringType),
        StructField("businessUse", StringType),
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
        StructField("updateFrequency", StringType),
        StructField("termsOfUse", ArrayType(StructType(Seq(
          StructField("url", StringType),
          StructField("name", StringType),
          StructField("dataAssetId", StringType)
        )))),
        StructField("documentation", ArrayType(StructType(Seq(
          StructField("url", StringType),
          StructField("name", StringType),
          StructField("dataAssetId", StringType)
        )))),
        StructField("sensitivityLabel", StringType),
        StructField("status", StringType),
        StructField("endorsed", BooleanType),
        StructField("additionalProperties", StructType(Seq(
          StructField("assetCount", IntegerType)
        ))),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        )))
      ))),
      StructField("after", StructType(Seq(
        StructField("id", StringType),
        StructField("name", StringType),
        StructField("domain", StringType),
        StructField("type", StringType),
        StructField("description", StringType),
        StructField("businessUse", StringType),
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
        StructField("updateFrequency", StringType),
        StructField("termsOfUse", ArrayType(StructType(Seq(
          StructField("url", StringType),
          StructField("name", StringType),
          StructField("dataAssetId", StringType)
        )))),
        StructField("documentation", ArrayType(StructType(Seq(
          StructField("url", StringType),
          StructField("name", StringType),
          StructField("dataAssetId", StringType)
        )))),
        StructField("sensitivityLabel", StringType),
        StructField("status", StringType),
        StructField("endorsed", BooleanType),
        StructField("additionalProperties", StructType(Seq(
          StructField("assetCount", IntegerType)
        ))),
        StructField("systemData", StructType(Seq(
          StructField("lastModifiedAt", StringType),
          StructField("lastModifiedBy", StringType),
          StructField("createdAt", StringType),
          StructField("createdBy", StringType),
          StructField("expiredAt", StringType),
          StructField("expiredBy", StringType)
        )))
      ))),
      StructField("related", StructType(Seq(
        StructField("dataAssetId", ArrayType(StringType))
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
    //StructField("_self", StringType),
    StructField("_etag", StringType),
    //StructField("_attachments", StringType),
    StructField("_ts", LongType)
  ))
}
