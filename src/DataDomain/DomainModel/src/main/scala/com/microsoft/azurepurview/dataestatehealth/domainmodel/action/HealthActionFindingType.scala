package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.GenerateId
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, lower, trim}
import org.apache.spark.sql.types.StringType
import org.apache.spark.sql.{DataFrame, SparkSession}

class HealthActionFindingType(spark: SparkSession, logger: Logger) {
  def processDataFindingType(df: DataFrame, schema: org.apache.spark.sql.types.StructType): DataFrame = {
    try {

      val dfProcessUpsert = df.select(col("JObject.findingType").alias("FindingTypeDisplayName"))

      var dfProcess = dfProcessUpsert.filter("FindingTypeDisplayName IS NOT NULL").distinct()

      val generateIdColumn = new GenerateId()
      dfProcess = generateIdColumn.IdGenerator(dfProcess, List("FindingTypeDisplayName"), "FindingTypeId")

      dfProcess = dfProcess.select(col("FindingTypeId").cast(StringType)
        , col("FindingTypeDisplayName").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema = schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing HealthActionFindingType Data: ${e.getMessage}")
        logger.error(s"Error Processing HealthActionFindingType Data: ${e.getMessage}")
        throw e
    }
  }
}
