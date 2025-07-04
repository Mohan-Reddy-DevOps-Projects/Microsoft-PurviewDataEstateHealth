package com.microsoft.azurepurview.dataestatehealth.domainmodel.cde

import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.Validator
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, row_number, when}
import org.apache.spark.sql.types.{LongType, TimestampType}

class CDE(spark: SparkSession, logger:Logger) {
  def processCDE(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{
      val dfProcessUpsert = df.select(col("operationType").alias("OperationType")
        ,col("_ts").alias("EventProcessingTime")
        ,col("payload.after.name").alias("Name")
        ,col("payload.after.dataType").alias("DataType")
        ,col("payload.after.status").alias("Status")
        ,col("payload.after.description").alias("Description")
        ,col("payload.after.domain").alias("BusinessDomainId")
        ,col("payload.after.id").alias("CDEId")
        ,col("payload.after.systemData.createdBy").alias("CreatedBy")
        ,col("payload.after.systemData.createdAt").alias("CreatedAt")
        ,col("payload.after.systemData.lastModifiedBy").alias("LastModifiedBy")
        ,col("payload.after.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Create' or OperationType=='Update'")

      val DeleteIsEmpty = df.filter("OperationType=='Delete'").isEmpty
      var dfProcess=dfProcessUpsert
      if (!DeleteIsEmpty) {
        val dfProcessDelete = df.select(col("operationType").alias("OperationType")
          ,col("_ts").alias("EventProcessingTime")
          ,col("payload.before.name").alias("Name")
          ,col("payload.before.dataType").alias("DataType")
          ,col("payload.before.status").alias("Status")
          ,col("payload.before.description").alias("Description")
          ,col("payload.before.domain").alias("BusinessDomainId")
          ,col("payload.before.id").alias("CDEId")
          ,col("payload.before.systemData.createdBy").alias("CreatedBy")
          ,col("payload.before.systemData.createdAt").alias("CreatedAt")
          ,col("payload.before.systemData.lastModifiedBy").alias("LastModifiedBy")
          ,col("payload.before.systemData.lastModifiedAt").alias("LastModifiedAt")).filter("OperationType=='Delete'")
        dfProcess = dfProcessUpsert.unionAll(dfProcessDelete)
      }
      else{
        dfProcess = dfProcessUpsert
      }

      dfProcess = dfProcess.filter(s"""CDEId IS NOT NULL
                                      | AND Name IS NOT NULL""".stripMargin).distinct()

      dfProcess = dfProcess.select(col("CDEId").alias("CriticalDataElementId")
        ,col("Name").alias("CriticalDataElementDisplayName")
        ,col("Description").alias("CriticalDataElementDescription")
        ,col("Status").alias("CriticalDataElementStatus")
        ,col("DataType").alias("ExpectedDataType")
        ,col("CreatedAt").alias("CreatedDatetime").cast(TimestampType)
        ,col("CreatedBy").alias("CreatedByUserId")
        ,col("LastModifiedAt").alias("ModifiedDateTime").cast(TimestampType)
        ,col("LastModifiedBy").alias("ModifiedByUserId")
        ,col("EventProcessingTime").cast(LongType)
        ,col("OperationType")
      )

      val windowSpec = Window.partitionBy("CriticalDataElementId").orderBy(col("ModifiedDateTime").desc,
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
      validator.validateDataFrame(dfProcessed,"CriticalDataElementId is null or CriticalDataElementDisplayName is null")

      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing CDE Data: ${e.getMessage}")
        logger.error(s"Error Processing CDE Data: ${e.getMessage}")
        throw e
    }
  }
}
