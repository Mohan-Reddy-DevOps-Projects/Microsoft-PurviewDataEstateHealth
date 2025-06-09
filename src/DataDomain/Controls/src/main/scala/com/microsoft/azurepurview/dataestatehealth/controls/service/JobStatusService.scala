package com.microsoft.azurepurview.dataestatehealth.controls.service

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.Reader
import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.cosmosdb.CosmosWriter
import com.microsoft.azurepurview.dataestatehealth.controls.common.CommonUtils
import com.microsoft.azurepurview.dataestatehealth.controls.constants.Constants
import com.microsoft.azurepurview.dataestatehealth.controls.model.ControlJobContractSchema
import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SaveMode, SparkSession}
import org.apache.spark.sql.functions._

import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

/**
 * Service for updating only the jobStatus field in existing control job records
 * All other fields are preserved exactly as they were
 *
 * @param spark The SparkSession to use
 * @param logger Optional logger instance
 */
class JobStatusService(
  spark: SparkSession,
  private val logger: Logger = Logger.getLogger(classOf[JobStatusService])
) {

  import spark.implicits._

  /**
   * Updates ONLY the jobStatus field in existing control job records
   * ALL OTHER FIELDS ARE PRESERVED EXACTLY AS THEY WERE
   *
   * @param controlJobQueries DataFrame containing the complete control job records with ALL fields
   * @param status The job status to set
   * @param errorMessage Optional error message for failed jobs
   */
  def updateJobStatus(
    controlJobQueries: DataFrame,
    status: JobStatus.JobStatus,
    errorMessage: Option[String] = None
  ): Unit = {
    if (controlJobQueries == null || controlJobQueries.isEmpty) {
      logger.warn("No control job queries provided for status update")
      return
    }
    
    // Map JobStatus enum to string values
    val statusString = status match {
      case JobStatus.Started => "Running"
      case JobStatus.Completed => "Completed"
      case JobStatus.Failed => "Failed"
      case _ => status.toString
    }
    
    logger.info(s"Updating ONLY jobStatus field to '$statusString' - ALL OTHER FIELDS WILL BE PRESERVED")
    
    // Update ONLY the jobStatus column - all other columns remain unchanged
    val updatedRecords = controlJobQueries.withColumn("jobStatus", lit(statusString))
    
    // Show the schema to confirm all fields are present
    logger.info(s"Updated DataFrame contains ${updatedRecords.columns.length} columns (same as original)")
    
    // Write the complete records back to Cosmos DB (upsert based on 'id' field)
    writeToCosmosDB(updatedRecords)
  }

  /**
   * Writes the updated records back to Cosmos DB
   * The records contain ALL original fields plus the updated jobStatus
   *
   * @param updatedRecords DataFrame with updated jobStatus but all other fields preserved
   */
  private def writeToCosmosDB(updatedRecords: DataFrame): Unit = {
    CommonUtils.safeExecute("writing updated records to Cosmos DB", logger) {
      logger.info("Writing complete records (with updated jobStatus) back to Cosmos DB")
      
      val cosmosWriter = new CosmosWriter(spark)
      cosmosWriter.writeToCosmosDB(
        df = updatedRecords,
        database = Constants.Cosmos.DATABASE,
        container = Constants.Cosmos.CONTAINER,
        saveMode = SaveMode.Append, // Upserts based on 'id' field
        entityName = "controlJobStatusUpdate"
      )
      
      val recordCount = updatedRecords.count()
      logger.info(s"Successfully updated jobStatus for $recordCount records - all other fields preserved")
    }
  }

  /**
   * Convenience method to update status using account ID and job run GUID
   * Reads the complete records first, then updates only jobStatus
   *
   * @param accountId The account ID
   * @param jobRunGuid The job run GUID
   * @param status The job status to set
   * @param errorMessage Optional error message for failed jobs
   */
  def updateJobStatusByIds(
    accountId: String,
    jobRunGuid: String,
    status: JobStatus.JobStatus,
    errorMessage: Option[String] = None
  ): Unit = {
    CommonUtils.safeExecute("reading and updating control job status", logger) {
      logger.info(s"Reading complete control job records for accountId: $accountId, jobRunGuid: $jobRunGuid")
      
      // Read the COMPLETE control job records from Cosmos DB (with ALL fields)
      val reader = new Reader(spark, logger)
      val contractSchema = new ControlJobContractSchema().controlJobContractSchema
      
      val controlJobQueries = reader.readCosmosData(
        contractSchema,
        "",
        accountId,
        Constants.Cosmos.CONTAINER,
        "",
        "",
        Constants.Cosmos.DATABASE,
        "",
        jobRunGuid
      )
      
      if (controlJobQueries != null && !controlJobQueries.isEmpty) {
        logger.info(s"Found ${controlJobQueries.count()} records to update")
        updateJobStatus(controlJobQueries, status, errorMessage)
      } else {
        logger.error(s"No control job records found for accountId: $accountId, jobRunGuid: $jobRunGuid")
      }
    }
  }
} 