package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, lower, trim}
import org.apache.spark.sql.types.{LongType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class HealthAction(spark: SparkSession, logger: Logger) {
  def processAction(df: DataFrame, schema: org.apache.spark.sql.types.StructType, adlsTargetDirectory: String): DataFrame = {
    try {

      val dfFindingType = spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/HealthActionFindingType"))

      var dfProcess = df.select(col("_ts").alias("EventProcessingTime")
        , col("JObject.category").alias("Category")
        , col("JObject.severity").alias("Severity")
        , col("JObject.findingName").alias("FindingDisplayName")
        , col("JObject.reason").alias("Reason")
        , col("JObject.recommendation").alias("Recommendation")
        , col("JObject.findingType").alias("FindingType")
        , col("JObject.targetEntityType").alias("TargetEntityType")
        , col("JObject.targetEntityId").alias("TargetEntityId")
        , col("JObject.domainId").alias("BusinessDomainId")
        , col("JObject.extraProperties.type").alias("Type")
        , col("JObject.status").alias("Status")
        , col("JObject.id").alias("ActionId")
        , col("JObject.systemData.createdBy").alias("CreatedBy")
        , col("JObject.systemData.createdAt").alias("CreatedAt")
        , col("JObject.systemData.lastModifiedBy").alias("LastModifiedBy")
        , col("JObject.systemData.lastModifiedAt").alias("LastModifiedAt"))


      dfProcess = dfProcess.filter(s"""ActionId IS NOT NULL""".stripMargin).distinct()

      dfProcess =  dfProcess
        .join(dfFindingType, lower(trim(dfProcess("FindingType"))) === lower(trim(dfFindingType("FindingTypeDisplayName"))))

      dfProcess = dfProcess.select(col("ActionId")
        , col("FindingDisplayName")
        , col("FindingTypeId")
        , col("BusinessDomainId")
        , col("Category")
        , col("Severity")
        , col("Reason")
        , col("Recommendation")
        , col("TargetEntityType")
        , col("TargetEntityId")
        , col("Type")
        , col("Status")
        , col("CreatedAt").alias("CreatedDatetime").cast(TimestampType)
        , col("CreatedBy").alias("CreatedByUserId")
        , col("LastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
        , col("LastModifiedBy").alias("ModifiedByUserId")
        , col("EventProcessingTime").cast(LongType)
      )

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema = schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed, "ActionId is null")
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing HealthAction Data: ${e.getMessage}")
        logger.error(s"Error Processing HealthAction Data: ${e.getMessage}")
        throw e
    }
  }
}
