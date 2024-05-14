package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{SCDType2Processor, Validator}
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions.{col}
import org.apache.spark.sql.types.{LongType, StringType}

class DimDQJobType (spark: SparkSession, logger:Logger){
  def processDimDQJobType(schema: org.apache.spark.sql.types.StructType):DataFrame= {
    try {

      val data = Seq(
        Row(1L, "DQ"),
        Row(2L, "MDQ"),
        Row(3L, "NOT-AVAILABLE")
      )
      var dfProcess = spark.createDataFrame(spark.sparkContext.parallelize(data), schema)

      dfProcess = dfProcess.select(col("JobTypeId").cast(LongType)
        ,col("JobTypeDisplayName").cast(StringType))

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""JobTypeId is null
                            | or JobTypeDisplayName is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcess,filterString)

      dfProcessed

    } catch {
      case e: Exception =>
        println(s"Error Processing DimDQJobType Dimension: ${e.getMessage}")
        logger.error(s"Error Processing DimDQJobType Dimension: ${e.getMessage}")
        throw e
    }
  }
}
