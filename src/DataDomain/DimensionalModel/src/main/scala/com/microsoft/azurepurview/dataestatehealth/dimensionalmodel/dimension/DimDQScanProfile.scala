package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{GenerateId, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, lit, row_number, sha2}
import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType}

class DimDQScanProfile (spark: SparkSession, logger:Logger){
  def processEmptyDimDQScanProfile(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimDQScanProfileNASchema = StructType(
        Array(
          StructField("RuleOriginDisplayName", StringType, nullable = false),
          StructField("RuleAppliedOn", StringType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE","NOT-AVAILABLE")
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDQScanProfileNASchema)

      val windowSpec = Window.partitionBy().orderBy("RuleOriginDisplayName","RuleAppliedOn")
      naDF = naDF.withColumn("DQScanProfileId", row_number().over(windowSpec))

      var dfProcess = naDF
      dfProcess = dfProcess.select(col("DQScanProfileId").cast(LongType)
        ,col("RuleOriginDisplayName").cast(StringType)
        ,col("RuleAppliedOn").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing EmptyDimDQScanProfile Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimDQScanProfile Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimDQScanProfile(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame= {
    try {
      var dfProcess = df.select(col("RuleOriginDisplayName").alias("RuleOriginDisplayName")
        ,col("RuleTargetObjectType").alias("RuleAppliedOn")
      ).distinct()
      //Adding NOT AVAILABLE Value Option
      val dimDQScanProfileNASchema = StructType(
        Array(
          StructField("RuleOriginDisplayName", StringType, nullable = false),
          StructField("RuleAppliedOn", StringType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE","NOT-AVAILABLE")
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDQScanProfileNASchema)

      dfProcess = dfProcess.union(naDF.select(
        col("RuleOriginDisplayName"),
        col("RuleAppliedOn")
      ))

      val windowSpec = Window.partitionBy().orderBy("RuleOriginDisplayName","RuleAppliedOn")
      dfProcess = dfProcess.withColumn("DQScanProfileId", row_number().over(windowSpec))

      dfProcess = dfProcess.select(col("DQScanProfileId").cast(LongType)
        ,col("RuleOriginDisplayName").cast(StringType)
        ,col("RuleAppliedOn").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DQScanProfileId is null
                            | or RuleOriginDisplayName is null
                            | or RuleAppliedOn is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)
      dfProcessed
    } catch {
      case e: Exception =>
        println(s"Error Processing DimDQScanProfile Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDQScanProfile Dimension: ${e.getMessage}")
        throw e
    }
  }
}
