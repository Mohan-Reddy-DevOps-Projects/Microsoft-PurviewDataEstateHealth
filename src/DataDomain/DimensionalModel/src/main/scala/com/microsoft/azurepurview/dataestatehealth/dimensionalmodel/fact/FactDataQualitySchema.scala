package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact
import org.apache.spark.sql.types.{BooleanType, DecimalType, IntegerType, LongType, StringType, StructField, StructType, TimestampType}
class FactDataQualitySchema {
  val factDataQualitySchemaSchema: StructType = StructType(
    Array(
      StructField("FactDataQualityId", IntegerType, nullable = false),
      StructField("DQJobSourceId", StringType, nullable = false),
      StructField("DQRuleId", LongType, nullable = false),
      StructField("RuleScanCompletionDatetime", TimestampType, nullable = false),
      StructField("DEHLastProcessedDatetime", TimestampType, nullable = false),
      StructField("BusinessDomainId", StringType, nullable = false),
      StructField("DataProductId", StringType, nullable = false),
      StructField("DataAssetId", StringType, nullable = false),
      StructField("DataAssetColumnId", StringType, nullable = false),
      StructField("JobTypeId", LongType, nullable = false),
      StructField("DQRuleTypeId", LongType, nullable = false),
      StructField("DQScanProfileId", LongType, nullable = false),
      StructField("ScanCompletionDateId", LongType, nullable = false),
      StructField("DQOverallProfileQualityScore", DecimalType (18, 10), nullable = false),
      StructField("DQPassedCount", IntegerType, nullable = false),
      StructField("DQFailedCount", IntegerType, nullable = false),
      StructField("DQIgnoredCount", IntegerType, nullable = false),
      StructField("DQEmptyCount", IntegerType, nullable = false),
      StructField("DQMiscastCount", IntegerType, nullable = false)
    )
  )
}