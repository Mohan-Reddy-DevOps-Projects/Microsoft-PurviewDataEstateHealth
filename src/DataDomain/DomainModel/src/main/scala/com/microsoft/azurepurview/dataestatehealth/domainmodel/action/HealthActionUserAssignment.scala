package com.microsoft.azurepurview.dataestatehealth.domainmodel.action

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.functions.{col, explode_outer}
import org.apache.spark.sql.{DataFrame, SparkSession}

class HealthActionUserAssignment(spark: SparkSession, logger: Logger) {
  def processHealthActionUserAssignment(df: DataFrame, schema: org.apache.spark.sql.types.StructType): DataFrame = {
    try {

      var dfProcess = df.select(col("JObject.id").alias("ActionId")
        , col("JObject.assignedTo").alias("AssignedToUserIds"))

      dfProcess = dfProcess.filter(s"""ActionId IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.withColumn("AssignedToUserId", explode_outer(col("AssignedToUserIds")))
        .select(col("ActionId")
          , col("AssignedToUserId")
        ).distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema = schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed, "ActionId is null")
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing HealthActionUserAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing HealthActionUserAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
