package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.GenerateId
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, lower, trim}
import org.apache.spark.sql.types.StringType

class HealthActionFindingSubType(spark: SparkSession, logger: Logger) {
  def processDataFindingSubType(df: DataFrame, schema: org.apache.spark.sql.types.StructType, adlsTargetDirectory: String): DataFrame = {
    try {

      val dfFindingType = spark.read.format("delta")
        .load(adlsTargetDirectory.concat("/HealthActionFindingType"))

      val dfProcessUpsert = df.select(col("JObject.findingType").alias("FindingTypeDisplayName")
        ,col("JObject.findingSubType").alias("FindingSubTypeDisplayName"))

      var dfProcess = dfProcessUpsert.filter("FindingSubTypeDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess, List("FindingSubTypeDisplayName"), "FindingSubTypeId")

      dfProcess = dfProcess.select(col("FindingSubTypeId").cast(StringType)
        , col("FindingSubTypeDisplayName").cast(StringType)
        , col("FindingTypeDisplayName").cast(StringType))

      val dfJoin = dfProcess
        .join(dfFindingType, lower(trim(dfProcess("FindingTypeDisplayName"))) === lower(trim(dfFindingType("FindingTypeDisplayName"))))
        .select(col("FindingSubTypeId")
          ,col("FindingSubTypeDisplayName")
          ,col("FindingTypeId"))

      val dfProcessed = spark.createDataFrame(dfJoin.rdd, schema = schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing HealthActionFindingSubType Data: ${e.getMessage}")
        logger.error(s"Error Processing HealthActionFindingSubType Data: ${e.getMessage}")
        throw e
    }
  }
}
