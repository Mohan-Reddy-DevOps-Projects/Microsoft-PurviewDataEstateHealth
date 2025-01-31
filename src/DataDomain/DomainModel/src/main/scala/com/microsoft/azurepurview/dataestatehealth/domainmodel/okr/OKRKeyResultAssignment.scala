package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, row_number, when}
import org.apache.spark.sql.types.{LongType, StringType, TimestampType}

class OKRKeyResultAssignment(spark: SparkSession, logger: Logger) {
  def processOKRKeyResultAssignment(adlsTargetDirectory: String, schema: org.apache.spark.sql.types.StructType): DataFrame = {
    try {

      val dfRelationship = spark.read.format("delta").load(adlsTargetDirectory.concat("/Relationship"))

      val dfOKRKeyResult = dfRelationship.select(col("SourceType")
        , col("SourceId")
        , col("TargetType")
        , col("TargetId")
        , col("ModifiedByUserId")
        , col("ModifiedDateTime")
        , col("EventProcessingTime")
        , col("OperationType")
      ).filter("SourceType='Objective' and TargetType='KeyResult'")

      val dfKeyResultOKR = dfRelationship.select(col("TargetType")
        , col("TargetId")
        , col("SourceType")
        , col("SourceId")
        , col("ModifiedByUserId")
        , col("ModifiedDateTime")
        , col("EventProcessingTime")
        , col("OperationType")
      ).filter("TargetType='Objective' and SourceType='KeyResult'")

      val dfProcess = dfOKRKeyResult.union(dfKeyResultOKR)
        .withColumn("OKRId", col("SourceId"))
        .withColumn("KeyResultId", col("TargetId"))

      val windowSpec = Window.partitionBy("OKRId","KeyResultId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)

      val dfLatest = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()
        .select(col("OKRId").cast(StringType)
          ,col("KeyResultId").cast(StringType)
          ,col("ModifiedDateTime").cast(TimestampType)
          ,col("ModifiedByUserId").cast(StringType)
          ,col("EventProcessingTime").cast(LongType)
          ,col("OperationType").cast(StringType)
        )

      val dfFiltered = dfLatest.filter(col("OperationType") =!= "Delete")

      val dfProcessed = spark.createDataFrame(dfFiltered.rdd, schema=schema)

      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing OKRKeyResultAssignment Data: ${e.getMessage}")
        logger.error(s"Error Processing OKRKeyResultAssignment Data: ${e.getMessage}")
        throw e
    }
  }
}
