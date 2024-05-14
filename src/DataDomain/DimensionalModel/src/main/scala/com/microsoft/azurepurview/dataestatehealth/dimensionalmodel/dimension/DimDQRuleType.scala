package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{GenerateId, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions.{col, lit, row_number, sha2}
import org.apache.spark.sql.types.{LongType, StringType, StructField, StructType}

class DimDQRuleType (spark: SparkSession, logger:Logger){
  def processEmptyDimDQRuleType(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimDQRuleTypeNASchema = StructType(
        Array(
          StructField("DQRuleTypeDisplayName", StringType, nullable = false),
          StructField("QualityDimension", StringType, nullable = false),
          StructField("QualityDimensionCustomIndicator", StringType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE","NOT-AVAILABLE","NOT-AVAILABLE")
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDQRuleTypeNASchema)
      naDF = naDF.withColumn("DQRuleTypeSourceId", sha2(col("DQRuleTypeDisplayName"), 256))

      val generateIdColumn = new GenerateId()
      naDF = generateIdColumn.IdGenerator(naDF, List("DQRuleTypeSourceId"), "DQRuleTypeSourceId")

      val windowSpec = Window.partitionBy().orderBy("DQRuleTypeSourceId")
      naDF = naDF.withColumn("DQRuleTypeId", row_number().over(windowSpec))

      var dfProcess = naDF
      dfProcess = dfProcess.select(col("DQRuleTypeId").cast(LongType)
        ,col("DQRuleTypeSourceId").cast(StringType)
        ,col("DQRuleTypeDisplayName").cast(StringType)
        ,col("QualityDimension").cast(StringType)
        ,col("QualityDimensionCustomIndicator").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed

    }
    catch {
      case e: Exception =>
        println(s"Error Processing EmptyDimDQRuleType Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimDQRuleType Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimDQRuleType(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame= {
    try {
      var dfProcess = df.select(col("RuleTypeId").alias("DQRuleTypeSourceId")
        ,col("RuleTypeDisplayName").alias("DQRuleTypeDisplayName")
        ,col("DimensionDisplayName").alias("QualityDimension")
      ).withColumn("QualityDimensionCustomIndicator",lit(null:StringType)).distinct()

      //Adding NOT AVAILABLE Value Option
      val dimDQRuleTypeNASchema = StructType(
        Array(
          StructField("DQRuleTypeDisplayName", StringType, nullable = false),
          StructField("QualityDimension", StringType, nullable = false),
          StructField("QualityDimensionCustomIndicator", StringType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE","NOT-AVAILABLE","NOT-AVAILABLE")
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDQRuleTypeNASchema)
      naDF = naDF.withColumn("DQRuleTypeSourceId", sha2(col("DQRuleTypeDisplayName"), 256))
      val generateIdColumn = new GenerateId()
      naDF = generateIdColumn.IdGenerator(naDF, List("DQRuleTypeSourceId"), "DQRuleTypeSourceId")

      dfProcess = dfProcess.union(naDF.select(
        col("DQRuleTypeSourceId"),
        col("DQRuleTypeDisplayName"),
        col("QualityDimension"),
        col("QualityDimensionCustomIndicator")
      ))

      val windowSpec = Window.partitionBy().orderBy("DQRuleTypeSourceId")
      dfProcess = dfProcess.withColumn("DQRuleTypeId", row_number().over(windowSpec))

      dfProcess = dfProcess.select(col("DQRuleTypeId").cast(LongType)
        ,col("DQRuleTypeSourceId").cast(StringType)
        ,col("DQRuleTypeDisplayName").cast(StringType)
        ,col("QualityDimension").cast(StringType)
      ,col("QualityDimensionCustomIndicator").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DQRuleTypeId is null
                            | or DQRuleTypeSourceId is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)
      dfProcessed
    } catch {
      case e: Exception =>
        println(s"Error Processing DimDQRuleType Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDQRuleType Dimension: ${e.getMessage}")
        throw e
    }
  }
}

