package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class DataQualityRuleSchema {
  val dataQualityRuleSchema: StructType = StructType(
    Array(
      StructField("RuleId", StringType, nullable = false),
      StructField("SourceRuleId", StringType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("DataProductId", StringType, nullable = false),
      StructField("DataAssetId", StringType, nullable = false),
      StructField("RuleTypeId", StringType, nullable = false),
      StructField("RuleOriginDisplayName", StringType, nullable = true),
      StructField("RuleTargetObjectType", StringType, nullable = true),
      StructField("RuleDisplayName", StringType, nullable = true),
      StructField("RuleDescription", StringType, nullable = true),
      StructField("Status", StringType, nullable = true),
      StructField("AccountId", StringType, nullable = false),
      StructField("CreatedDatetime", TimestampType, nullable = true),
      StructField("CreatedByUserId", StringType, nullable = true),
      StructField("ModifiedDateTime", TimestampType, nullable = true),
      StructField("ModifiedByUserId", StringType, nullable = true),
      StructField("EventProcessingTime", LongType, nullable = false),
      StructField("OperationType", StringType, nullable = false)
    )
  )
}