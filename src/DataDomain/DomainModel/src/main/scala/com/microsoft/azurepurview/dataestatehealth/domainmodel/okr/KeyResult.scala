package com.microsoft.azurepurview.dataestatehealth.domainmodel.okr

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number, when}
import org.apache.spark.sql.types.{LongType, TimestampType}

class KeyResult(spark: SparkSession, logger:Logger) {
  def processKeyResult(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("accountId").alias("AccountId")
        ,col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.id").alias("KeyResultId")
        ,col("payload.after.definition").alias("KeyResultDefintion")
        ,col("payload.after.domainId").alias("BusinessDomainId")
        ,col("payload.after.status").alias("Status")
        ,col("payload.after.progress").alias("Progress")
        ,col("payload.after.goal").alias("Goal")
        ,col("payload.after.max").alias("Max")
        ,col("payload.after.okrid").alias("OKRId")
        ,col("payload.after.systemData.createdBy").alias("CreatedBy")
        ,col("payload.after.systemData.createdAt").alias("CreatedAt")
        ,col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
        ,col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Create' or OperationType=='Update'")

      val DeleteIsEmpty = df.filter("OperationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("accountId").alias("AccountId")
          ,col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.after.id").alias("KeyResultId")
          ,col("payload.after.definition").alias("KeyResultDefintion")
          ,col("payload.after.domainId").alias("BusinessDomainId")
          ,col("payload.after.status").alias("Status")
          ,col("payload.after.progress").alias("Progress")
          ,col("payload.after.goal").alias("Goal")
          ,col("payload.after.max").alias("Max")
          ,col("payload.after.okrid").alias("OKRId")
          ,col("payload.after.systemData.createdBy").alias("CreatedBy")
          ,col("payload.after.systemData.createdAt").alias("CreatedAt")
          ,col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
          ,col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else{
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter(s"""KeyResultId IS NOT NULL
                                      | AND KeyResultDefintion IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("KeyResultId")
        ,col("KeyResultDefintion")
        ,col("Status")
        ,col("Progress")
        ,col("Goal")
        ,col("Max")
        ,col("OKRId")
        ,col("AccountId")
        ,col("CreatedAt").alias("CreatedDatetime").cast(TimestampType)
        ,col("CreatedBy").alias("CreatedByUserId")
        ,col("LastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
        ,col("LastModifiedBy").alias("ModifiedByUserId")
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType")
        ,col("BusinessDomainId")
      )

      val windowSpec = Window.partitionBy("KeyResultId").orderBy(col("ModifiedDateTime").desc,
        when(col("OperationType") === "Create", 1)
          .when(col("OperationType") === "Update", 2)
          .when(col("OperationType") === "Delete", 3)
          .otherwise(4)
          .desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")
        .distinct()
      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,"KeyResultId is null or KeyResultDefintion is null or AccountId is null")
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
