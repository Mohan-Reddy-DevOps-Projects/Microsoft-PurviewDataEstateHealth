package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number, when}
import org.apache.spark.sql.types.{LongType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class Action(spark: SparkSession, logger:Logger) {
  def processAction(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      var dfProcess = df.select(col("accountId").alias("AccountId")
        ,col("_ts").alias("EventProcessingTime")
        ,col("JObject.category").alias("Category")
        ,col("JObject.severity").alias("Severity")
        ,col("JObject.findingId").alias("FindingId")
        ,col("JObject.findingName").alias("FindingName")
        ,col("JObject.reason").alias("Reason")
        ,col("JObject.recommendation").alias("Recommendation")
        ,col("JObject.findingType").alias("FindingType")
        ,col("JObject.findingSubType").alias("FindingSubType")
        ,col("JObject.targetEntityType").alias("TargetEntityType")
        ,col("JObject.domainId").alias("BusinessDomainId")
        ,col("JObject.extraProperties.type").alias("Type")
        ,col("JObject.status").alias("Status")
        ,col("JObject.id").alias("ActionId")
        ,col("JObject.systemData.createdBy").alias("CreatedBy")
        ,col("JObject.systemData.createdAt").alias("CreatedAt")
        ,col("JObject.systemData.lastModifiedBy").alias("LastModifiedBy")
        ,col("JObject.systemData.lastModifiedAt").alias("LastModifiedAt"))


      dfProcess = dfProcess.filter(s"""ActionId IS NOT NULL
                                      | AND AccountId IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("ActionId")
        ,col("AccountId")
        ,col("BusinessDomainId")
        ,col("Category")
        ,col("Severity")
        ,col("FindingId")
        ,col("FindingName")
        ,col("Reason")
        ,col("Recommendation")
        ,col("FindingType")
        ,col("FindingSubType")
        ,col("TargetEntityType")
        ,col("Type")
        ,col("Status")
        ,col("CreatedAt").alias("CreatedDatetime").cast(TimestampType)
        ,col("CreatedBy").alias("CreatedByUserId")
        ,col("LastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
        ,col("LastModifiedBy").alias("ModifiedByUserId")
        ,col("EventProcessingTime").cast(LongType)
      )

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,"ActionId is null or AccountId is null")
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing Action Data: ${e.getMessage}")
        logger.error(s"Error Processing Action Data: ${e.getMessage}")
        throw e
    }
  }
}
