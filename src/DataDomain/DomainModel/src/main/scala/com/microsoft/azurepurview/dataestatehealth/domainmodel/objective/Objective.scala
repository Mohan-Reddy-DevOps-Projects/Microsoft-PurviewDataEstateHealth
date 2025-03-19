package com.microsoft.azurepurview.dataestatehealth.domainmodel.objective

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{coalesce, col, lit, lower, row_number, trim, when}
import org.apache.spark.sql.types.{BooleanType, IntegerType, LongType, StringType, TimestampType}

import java.sql.Timestamp

class Objective (spark: SparkSession, logger:Logger) {
  def processObjective(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("ObjectiveId")
        ,col("payload.after.definition").alias("ObjectiveDisplayName")
        ,col("payload.after.domain").alias("BusinessDomainId")
        ,col("payload.after.status").alias("Status")
        ,col("payload.after.targetDate").alias("TargetDate")
        ,col("payload.after.systemData.createdBy").alias("CreatedBy")
        ,col("payload.after.systemData.createdAt").alias("CreatedAt")
        ,col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
        ,col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Create' or OperationType=='Update'")

      val DeleteIsEmpty = df.filter("OperationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.after.id").alias("ObjectiveId")
          ,col("payload.after.definition").alias("ObjectiveDisplayName")
          ,col("payload.after.domain").alias("BusinessDomainId")
          ,col("payload.after.status").alias("Status")
          ,col("payload.after.targetDate").alias("TargetDate")
          ,col("payload.after.systemData.createdBy").alias("CreatedBy")
          ,col("payload.after.systemData.createdAt").alias("CreatedAt")
          ,col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
          ,col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Delete'")
        dfProcess = dfProcessUpsert.union(dfProcessDelete)
      }
      else{
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter(s"""ObjectiveId IS NOT NULL
                                      | AND ObjectiveDisplayName IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("ObjectiveId")
        ,col("ObjectiveDisplayName")
        ,col("Status")
        ,col("TargetDate").cast(TimestampType)
        ,col("CreatedAt").alias("CreatedDatetime").cast(TimestampType)
        ,col("CreatedBy").alias("CreatedByUserId")
        ,col("LastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
        ,col("LastModifiedBy").alias("ModifiedByUserId")
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType")
        ,col("BusinessDomainId")
      )
      val windowSpec = Window.partitionBy("ObjectiveId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number","OperationType")
        .distinct()
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,"ObjectiveId is null or ObjectiveDisplayName is null")
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing Objective Data: ${e.getMessage}")
        logger.error(s"Error Processing Objective Data: ${e.getMessage}")
        throw e
    }
  }
}
