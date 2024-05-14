package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{Maintenance, Writer}
import io.delta.tables.DeltaTable
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
object FactMain {
  val logger: Logger = Logger.getLogger(getClass.getName)
  def main(AdlsTargetDirectory:String,ReProcessingThresholdInMins:Int,AccountId:String,JobRunGuid:String,spark:SparkSession): Unit = {
    try {
      println("In FactMain Spark Application!")
      logger.setLevel(Level.INFO)
      logger.info("Started the FactMain Main Spark Application!")

      println(
        s"""Received parameters:
           |Target ADLS Path - $AdlsTargetDirectory
           |Re-processing Threshold (in minutes) - $ReProcessingThresholdInMins
           |AccountId - $AccountId
           |JobRunGuid - $JobRunGuid""".stripMargin)

      // Processing FactDataQuality DeltaTable

      val writer = new Writer(logger)
      val VacuumOptimize= new Maintenance(spark,logger)
      val factDataQualitySchema = new FactDataQualitySchema().factDataQualitySchemaSchema
      val factDataQuality = new FactDataQuality(spark, logger)
      val df_factDataQualityProcessed = factDataQuality.processFactDataQuality(factDataQualitySchema,AdlsTargetDirectory)
      if (!df_factDataQualityProcessed.isEmpty) {
        writer.overWriteData(df_factDataQualityProcessed, AdlsTargetDirectory, "FactDataQuality", ReProcessingThresholdInMins)
        VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/FactDataQuality"))
        VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/FactDataQuality"),Some(df_factDataQualityProcessed),JobRunGuid, "FactDataQuality","")
      }
      else if (df_factDataQualityProcessed.isEmpty && !DeltaTable.isDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/FactDataQuality"))){
        val emptyDataFrame: DataFrame = spark.createDataFrame(spark.sparkContext.emptyRDD[Row], factDataQualitySchema)
        writer.overWriteData(emptyDataFrame,AdlsTargetDirectory, "FactDataQuality", ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/FactDataQuality"),Some(emptyDataFrame),JobRunGuid, "FactDataQuality","")
      }

      //Processing FactDataGovernanceScan
      val factDataGovernanceScanSchema = new FactDataGovernanceScanSchema().factDataGovernanceScanSchema
      val factDataGovernanceScan = new FactDataGovernanceScan(spark, logger)
      val df_factDataGovernanceScanProcessed = factDataGovernanceScan.processFactDataGovernanceScan(factDataGovernanceScanSchema,AdlsTargetDirectory)
      if (!df_factDataGovernanceScanProcessed.isEmpty) {
        writer.appendData(df_factDataGovernanceScanProcessed, AdlsTargetDirectory, "FactDataGovernanceScan", ReProcessingThresholdInMins)
        VacuumOptimize.processDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/FactDataGovernanceScan"))
        VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/FactDataGovernanceScan"),Some(df_factDataGovernanceScanProcessed),JobRunGuid, "FactDataGovernanceScan","")
      }
      else if (df_factDataGovernanceScanProcessed.isEmpty && !DeltaTable.isDeltaTable(AdlsTargetDirectory.concat("/DimensionalModel/FactDataGovernanceScan"))){
        val emptyDataFrame: DataFrame = spark.createDataFrame(spark.sparkContext.emptyRDD[Row], factDataGovernanceScanSchema)
        writer.overWriteData(emptyDataFrame,AdlsTargetDirectory, "FactDataGovernanceScan", ReProcessingThresholdInMins)
        VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory.concat("/DimensionalModel/FactDataGovernanceScan"),Some(emptyDataFrame),JobRunGuid, "FactDataGovernanceScan","")
      }

    } catch {
      case e: Exception =>
        println(s"Error In FactMain Spark Application!: ${e.getMessage}")
        logger.error(s"Error In FactMain Spark Application!: ${e.getMessage}")
        val VacuumOptimize = new Maintenance(spark,logger)
        VacuumOptimize.checkpointSentinel(AccountId,AdlsTargetDirectory,None,JobRunGuid, "FactMain",s"Error In FactMain Spark Application!: ${e.getMessage}")
        throw new IllegalArgumentException(s"Error In FactMain Main Application!: ${e.getMessage}")
    }
  }
}
