package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataAssetSchema {
  val dataAssetSchema: StructType = StructType(
    Array(
      StructField("DataAssetId", StringType, nullable = false),
      StructField("DataAssetTypeId", StringType, nullable = false),
      StructField("AssetDisplayName", StringType, nullable = false),
      StructField("AssetDescription", StringType, nullable = true),
      StructField("FullyQualifiedName", StringType, nullable = true),
      StructField("ScanSource", StringType, nullable = true),
      StructField("IsCertified", IntegerType, nullable = true),
      StructField("DataAssetLastUpdatedDatetime", TimestampType, nullable = true),
      StructField("DataAssetLastUpdatedByUserId", StringType, nullable = true),
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