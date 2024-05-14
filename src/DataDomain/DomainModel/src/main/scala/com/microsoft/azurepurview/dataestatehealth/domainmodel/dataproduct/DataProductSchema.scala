package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct
import org.apache.spark.sql.types.{BooleanType, IntegerType, StringType, StructField, StructType, TimestampType, LongType}
class DataProductSchema {
  val dataProductAssetSchema: StructType = StructType(
    Array(
      StructField("DataProductID", StringType, nullable = false),
      StructField("DataProductDisplayName", StringType, nullable = false),
      StructField("DataProductDescription", StringType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("DataProductTypeID", StringType, nullable = false),
      StructField("UseCases", StringType, nullable = true),
      StructField("SensitivityLabel", StringType, nullable = true),
      StructField("Endorsed", BooleanType, nullable = true),
      StructField("ExpiredFlag", IntegerType, nullable = false),
      StructField("ExpiredFlagLastModifiedDatetime", TimestampType, nullable = false),
      StructField("DataProductStatusID", StringType, nullable = false),
      StructField("DataProductStatusLastUpdatedDatetime", TimestampType, nullable = false),
      StructField("UpdateFrequencyId", StringType, nullable = true),
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