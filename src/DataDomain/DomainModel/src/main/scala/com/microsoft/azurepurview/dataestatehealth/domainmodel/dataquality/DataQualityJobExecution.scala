package com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.{DeltaTableProcessingCheck, GenerateId, Validator}
import io.delta.tables.DeltaTable
import org.apache.log4j.Logger
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions.{col, explode_outer, lit, row_number}
import org.apache.spark.sql.types.{IntegerType, StringType, TimestampType}

class DataQualityJobExecution (spark: SparkSession, logger:Logger){
  def processDataQualityJobExecution(df:DataFrame,schema: org.apache.spark.sql.types.StructType):DataFrame={
    try{

      var dfProcess = df.select(col("payloadDetails.domainmodel.payload.dataQualityFact").alias("DataQualityFact")
        ,col("payloadDetails.domainmodel.payload.jobId").alias("JobExecutionId")
        ,col("payloadDetails.jobStatus").alias("JobExecutionStatusDisplayName")
        ,col("payloadDetails.domainmodel.payload.jobType").alias("JobType")
        ,col("payloadDetails.domainmodel.payload.systemData.resultedAt").alias("ExecutionEndDatetime"))
        .filter("payloadDetails.domainmodel.eventSource=='DataQuality'")
        .filter("payloadDetails.domainmodel.payloadKind=='dataQualityFact'")
        .filter("lower(payloadDetails.domainmodel.operationType) IN ('upsert','failure','failed')")

      dfProcess = dfProcess
        .withColumn("ScanTypeDisplayName",lit(null:StringType))
        .withColumn("JobCreationDatetime",lit(null:TimestampType))
        .withColumn("ExecutionStartDatetime",lit(null:TimestampType))
        .distinct()

      dfProcess = dfProcess.filter(s"""JobExecutionId IS NOT NULL
                                      | AND JobExecutionStatusDisplayName IS NOT NULL""".stripMargin).distinct()

      val windowSpec = Window.partitionBy("JobExecutionId").orderBy(col("ExecutionEndDatetime").desc)
      dfProcess = dfProcess.withColumn("row_number", row_number().over(windowSpec))
        .filter(col("row_number") === 1)
        .drop("row_number")

      dfProcess = dfProcess.select(col("JobExecutionId").cast(StringType)
        ,col("JobExecutionStatusDisplayName").cast(StringType)
        ,col("JobType").cast(StringType)
        ,col("ScanTypeDisplayName").alias("ScanTypeDisplayName").cast(StringType)
        ,col("JobCreationDatetime").alias("JobCreationDatetime").cast(TimestampType)
        ,col("ExecutionStartDatetime").alias("ExecutionStartDatetime").cast(TimestampType)
        ,col("ExecutionEndDatetime").alias("ExecutionEndDatetime").cast(TimestampType))
        .distinct()

      val dfProcessed = spark.createDataFrame(dfProcess.rdd, schema=schema)
      val filterString = s"""JobExecutionId is null
                            | or JobExecutionStatusDisplayName is null""".stripMargin

      val validator = new Validator()
      validator.validateDataFrame(dfProcessed,filterString)
      dfProcessed
    }
    catch {
      case e: Exception =>
        println(s"Error Processing DataQualityJobExecution Data: ${e.getMessage}")
        logger.error(s"Error Processing DataQualityJobExecution Data: ${e.getMessage}")
        throw e
    }
  }
}
