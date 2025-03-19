package com.microsoft.azurepurview.dataestatehealth.domainmodel.objective

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number, when}
import org.apache.spark.sql.types.{IntegerType, LongType, StringType, TimestampType}
import org.apache.spark.sql.{DataFrame, SparkSession}

class KeyResult(spark: SparkSession, logger: Logger) {

  def getObjectiveKeyResultMapping(adlsTargetDirectory: String): DataFrame ={
    val dfRelationship = spark.read.format("delta").load(adlsTargetDirectory.concat("/Relationship"))

    val dfObjectiveKeyResult = dfRelationship.select(col("SourceType")
      , col("SourceId")
      , col("TargetType")
      , col("TargetId")
      , col("ModifiedDateTime")
      , col("EventProcessingTime")
      , col("OperationType")
    ).filter("SourceType='Objective' and TargetType='KeyResult'")

    val dfKeyResultObjective = dfRelationship.select(col("TargetType")
      , col("TargetId")
      , col("SourceType")
      , col("SourceId")
      , col("ModifiedDateTime")
      , col("EventProcessingTime")
      , col("OperationType")
    ).filter("TargetType='Objective' and SourceType='KeyResult'")

    val dfProcess = dfObjectiveKeyResult.union(dfKeyResultObjective)
      .withColumn("ObjectiveId", col("SourceId"))
      .withColumn("KeyResultId", col("TargetId"))

    val windowSpec = Window.partitionBy("ObjectiveId","KeyResultId").orderBy(col("ModifiedDateTime").desc,
      when(col("OperationType") === "Create", 1)
        .when(col("OperationType") === "Update", 2)
        .when(col("OperationType") === "Delete", 3)
        .otherwise(4)
        .desc)

    val dfLatest = dfProcess.withColumn("row_number", row_number().over(windowSpec))
      .filter(col("row_number") === 1)
      .drop("row_number")
      .distinct()
      .select(col("ObjectiveId").cast(StringType)
        ,col("KeyResultId").cast(StringType)
      )

    dfLatest.filter(col("OperationType") =!= "Delete")

  }

  def processKeyResult(df: DataFrame, schema: org.apache.spark.sql.types.StructType,
                       adlsTargetDirectory: String): DataFrame = {
    try {
      val dfProcessUpsert = df.select(col("operationType").alias("OperationType")
        , col("_ts").alias("EventProcessingTime")
        , col("payload.after.id").alias("KeyResultId")
        , col("payload.after.definition").alias("KeyResultDisplayName")
        , col("payload.after.status").alias("Status")
        , col("payload.after.progress").alias("Progress")
        , col("payload.after.goal").alias("Goal")
        , col("payload.after.max").alias("Maximum")
        , col("payload.after.systemData.createdBy").alias("CreatedBy")
        , col("payload.after.systemData.createdAt").alias("CreatedAt")
        , col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
        , col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Create' or OperationType=='Update'")

      val DeleteIsEmpty = df.filter("OperationType=='Delete'").isEmpty
      var dfProcess = dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("operationType").alias("OperationType")
          , col("_ts").alias("EventProcessingTime")
          , col("payload.after.id").alias("KeyResultId")
          , col("payload.after.definition").alias("KeyResultDisplayName")
          , col("payload.after.status").alias("Status")
          , col("payload.after.progress").alias("Progress")
          , col("payload.after.goal").alias("Goal")
          , col("payload.after.max").alias("Maximum")
          , col("payload.after.systemData.createdBy").alias("CreatedBy")
          , col("payload.after.systemData.createdAt").alias("CreatedAt")
          , col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
          , col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else {
        dfProcess = dfProcessUpsert
      }

      val dfObjectiveKeyResultMapping = getObjectiveKeyResultMapping(adlsTargetDirectory)

      dfProcess = dfProcess
        .filter(s"""KeyResultId IS NOT NULL AND KeyResultDisplayName IS NOT NULL""".stripMargin)
        .distinct()
        .join(dfObjectiveKeyResultMapping,"KeyResultId")
        .select(col("KeyResultId")
          , col("ObjectiveId")
          , col("KeyResultDisplayName")
          , col("Status")
          , col("Progress").cast(IntegerType)
          , col("Goal").cast(IntegerType)
          , col("Maximum").cast(IntegerType)
          , col("CreatedAt").alias("CreatedDatetime").cast(TimestampType)
          , col("CreatedBy").alias("CreatedByUserId")
          , col("LastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
          , col("LastModifiedBy").alias("ModifiedByUserId")
          , col("EventProcessingTime").cast(LongType)
          , col("OperationType")
        )

      val windowSpec = Window.partitionBy("KeyResultId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number","OperationType")
        .distinct()
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema = schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed, "KeyResultId is null or KeyResultDisplayName is null")
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing KeyResult Data: ${e.getMessage}")
        logger.error(s"Error Processing KeyResult Data: ${e.getMessage}")
        throw e
    }
  }
}
