package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{GenerateId, SCDType2Processor, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions.{col, lit, row_number, sha2}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, StructField, StructType, TimestampType}

class DimDQRuleName (spark: SparkSession, logger:Logger){
  def processEmptyDimDQRuleName(schema: org.apache.spark.sql.types.StructType):DataFrame={
    try {
      val dimDQRuleNameNASchema = StructType(
        Array(
          StructField("DQRuleNameDisplayName", StringType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE")
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDQRuleNameNASchema)
      naDF = naDF.withColumn("DQRuleNameSourceId", sha2(col("DQRuleNameDisplayName"), 256))
        .withColumn("DQRuleNameId", sha2(col("DQRuleNameDisplayName"), 256))

      val generateIdColumn = new GenerateId()
      naDF = generateIdColumn.IdGenerator(naDF, List("DQRuleNameSourceId"), "DQRuleNameSourceId")
      naDF = generateIdColumn.IdGenerator(naDF, List("DQRuleNameId"), "DQRuleNameId")

      val windowSpec = Window.partitionBy().orderBy("DQRuleNameId")
      naDF = naDF.withColumn("DQRuleId", row_number().over(windowSpec))

      var dfProcess = naDF
      dfProcess = dfProcess.select(col("DQRuleId").cast(LongType)
        ,col("DQRuleNameId").cast(StringType)
        ,col("DQRuleNameSourceId").cast(StringType)
        ,col("DQRuleNameDisplayName").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      dfProcessed

    }
    catch {
      case e: Exception =>
        println(s"Error Processing EmptyDimDQRuleName Dimension: ${e.getMessage}")
        logger.error(s"Error Processing EmptyDimDQRuleName Dimension: ${e.getMessage}")
        throw e
    }
  }
  def processDimDQRuleName(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame= {
    try {
      var dfProcess = df.select(col("RuleId").alias("DQRuleNameId")
        ,col("SourceRuleId").alias("DQRuleNameSourceId")
      ).withColumn("DQRuleNameDisplayName",lit(null:StringType))
        .distinct()
      //Adding NOT AVAILABLE Value Option
      val dimDQRuleNameNASchema = StructType(
        Array(
          StructField("DQRuleNameDisplayName", StringType, nullable = false)
        )
      )
      val newRow = Row("NOT-AVAILABLE")
      var naDF = spark.createDataFrame(spark.sparkContext.parallelize(Seq(newRow)), dimDQRuleNameNASchema)
      naDF = naDF.withColumn("DQRuleNameSourceId", sha2(col("DQRuleNameDisplayName"), 256))
        .withColumn("DQRuleNameId", sha2(col("DQRuleNameDisplayName"), 256))
      val generateIdColumn = new GenerateId()
      naDF = generateIdColumn.IdGenerator(naDF, List("DQRuleNameSourceId"), "DQRuleNameSourceId")
      naDF = generateIdColumn.IdGenerator(naDF, List("DQRuleNameId"), "DQRuleNameId")

      dfProcess = dfProcess.union(naDF.select(
        col("DQRuleNameId"),
        col("DQRuleNameSourceId"),
        col("DQRuleNameDisplayName")
      ))

      val windowSpec = Window.partitionBy().orderBy("DQRuleNameId")
      dfProcess = dfProcess.withColumn("DQRuleId", row_number().over(windowSpec))

      dfProcess = dfProcess.select(col("DQRuleId").cast(LongType)
      ,col("DQRuleNameId").cast(StringType)
        ,col("DQRuleNameSourceId").cast(StringType)
        ,col("DQRuleNameDisplayName").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""DQRuleNameId is null
                            | or DQRuleNameSourceId is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)
      dfProcessed
    } catch {
      case e: Exception =>
        println(s"Error Processing DimDQRuleName Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDQRuleName Dimension: ${e.getMessage}")
        throw e
    }
  }
}
