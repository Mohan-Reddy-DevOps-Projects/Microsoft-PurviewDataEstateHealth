package com.microsoft.azurepurview.dataestatehealth.controls.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.Reader
import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.cosmosdb.CosmosWriter
import com.microsoft.azurepurview.dataestatehealth.controls.common.{ColdStartSoftCheck, CommonUtils}
import com.microsoft.azurepurview.dataestatehealth.controls.constants.Constants
import com.microsoft.azurepurview.dataestatehealth.controls.model.{ControlJobContractSchema, ControlRule, ControlSummarySchema, JobConfig, QueryResult}
import com.microsoft.azurepurview.dataestatehealth.controls.service.{ControlJobService, JobStatusService, QualityScoreService}
import com.microsoft.azurepurview.dataestatehealth.controls.util.ControlJobUtils
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.{DataFrame, Row, SaveMode, SparkSession}
import org.apache.spark.sql.functions._
import org.apache.spark.sql.types._

import scala.util.{Failure, Success, Try}
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter
import java.util.UUID

/**
 * Main entry point for Control Jobs processing.
 * Handles the orchestration of control job validation, execution, and result processing.
 */
object ControlJobExecutor {
  private val logger: Logger = Logger.getLogger(getClass.getName)
  
  /**
   * Main entry point for the application
   *
   * @param args Command line arguments
   * @param spark The active SparkSession
   * @param reProcessingThresholdInMins Reprocessing threshold in minutes
   */
  def main(args: Array[String], spark: SparkSession, reProcessingThresholdInMins: Int): Unit = {
    var jobStatusService: JobStatusService = null
    var config: JobConfig = null
    var controlJobQueries: DataFrame = null
    
    try {
      logger.setLevel(Level.INFO)
      logger.info("Started the ControlJobs Table Main Application")

      // Parse and validate configuration
      config = JobConfig.fromArgs(args, reProcessingThresholdInMins)
      
      if (!validateConfig(config)) {
        throw new IllegalArgumentException("Invalid configuration parameters")
      }
      
      logConfiguration(config)
      
      // Initialize job status service
      jobStatusService = new JobStatusService(spark, logger)
      
      // Validate connection to Cosmos DB
      validateAndProcess(spark, config, jobStatusService)
      
    } catch {
      case e: Exception =>
        val errorMessage = s"Error in ControlJobs Main application: ${e.getMessage}"
        logger.error(errorMessage, e)
        
        // Update job status to Failed if we have the necessary components
        if (jobStatusService != null && config != null) {
          try {
            jobStatusService.updateJobStatusByIds(config.accountId, config.jobRunGuid, JobStatus.Failed, Some(errorMessage))
            logger.info("Updated job status to Failed in Cosmos DB")
          } catch {
            case statusException: Exception =>
              logger.error(s"Failed to update job status to Failed: ${statusException.getMessage}", statusException)
          }
        }
        
        throw new RuntimeException(errorMessage, e)
    }
  }
  
  /**
   * Validates the configuration parameters
   *
   * @param config The JobConfig to validate
   * @return true if validation succeeds, false otherwise
   */
  private def validateConfig(config: JobConfig): Boolean = {
    var isValid = true
    
    if (!CommonUtils.validateStringParam(config.adlsTargetDirectory, "adlsTargetDirectory", logger)) {
      isValid = false
    }
    
    if (!CommonUtils.validateStringParam(config.accountId, "accountId", logger)) {
      isValid = false
    }
    
    if (!CommonUtils.validateStringParam(config.jobRunGuid, "jobRunGuid", logger)) {
      isValid = false
    }
    
    isValid
  }
  
  /**
   * Log configuration parameters
   *
   * @param config The JobConfig to log
   */
  private def logConfiguration(config: JobConfig): Unit = {
    logger.info(
      s"""Processing with parameters:
         |Target ADLS Path: ${config.adlsTargetDirectory}
         |Account ID: ${config.accountId}
         |Processing Type: ${config.refreshType}
         |Job Run GUID: ${config.jobRunGuid}
         |Re-Processing Threshold: ${config.reProcessingThresholdInMins} minutes""".stripMargin)
  }
  
  /**
   * Validate connection to Cosmos DB and process control jobs if validation succeeds
   *
   * @param spark The SparkSession to use
   * @param config The JobConfig containing processing parameters
   * @param jobStatusService The JobStatusService for tracking job status
   */
  private def validateAndProcess(spark: SparkSession, config: JobConfig, jobStatusService: JobStatusService): Unit = {
    val coldStartSoftCheck = new ColdStartSoftCheck(spark, logger)
    
    if (coldStartSoftCheck.validateCheckIn(Constants.Cosmos.CONTAINER, Constants.Cosmos.DATABASE)) {
      processControlJobs(spark, config, jobStatusService)
    } else {
      val errorMsg = "Cold start validation failed - unable to connect to Cosmos DB"
      logger.error(errorMsg)
      throw new RuntimeException(errorMsg)
    }
  }
  
  /**
   * Process control jobs by reading configurations from Cosmos DB and executing job queries
   *
   * @param spark The SparkSession to use
   * @param config The JobConfig containing processing parameters
   * @param jobStatusService The JobStatusService for tracking job status
   */
  private def processControlJobs(spark: SparkSession, config: JobConfig, jobStatusService: JobStatusService): Unit = {
    CommonUtils.safeExecuteWithTry("processing control jobs", logger) {
      // Read control job configurations from Cosmos DB
      logger.info(s"Reading control job queries from Cosmos DB for account ID: ${config.accountId}")
      val controlJobQueries = readControlJobQueries(spark, config.accountId, config.jobRunGuid)
      
      if (ControlJobUtils.isDataFrameEmpty(controlJobQueries)) {
        val errorMsg = "No control job queries found in Cosmos DB"
        logger.error(errorMsg)
        throw new RuntimeException(errorMsg)
      }

      // Update job status to Running/Started at the beginning of processing
      logger.info("Updating job status to Running for all control jobs")
      jobStatusService.updateJobStatus(controlJobQueries, JobStatus.Started)

      // Process control job queries and apply rules
      logger.info("Processing control job queries")
      val controlJobService = new ControlJobService(spark, logger)
      val queryResults = controlJobService.processJobQueries(controlJobQueries)
      
      if (queryResults.isEmpty) {
        logger.warn("No query results returned from processing")
        return
      }
      
      // Apply rules and generate summaries for each result
      logger.info(s"Processing ${queryResults.size} control jobs")
      processQueryResults(queryResults, config, spark)
      
      // Update job status to Completed upon successful completion
      logger.info("Updating job status to Completed for all control jobs")
      jobStatusService.updateJobStatus(controlJobQueries, JobStatus.Completed)
      
      logger.info("Successfully completed processing all control jobs with status: Completed")
    } match {
      case Failure(e) => 
        throw new RuntimeException(s"Failed to process control jobs: ${e.getMessage}", e)
      case _ => // Success case
    }
  }
  
  /**
   * Read control job configurations from Cosmos DB
   *
   * @param spark The SparkSession to use
   * @param accountId The account ID to read data for
   * @param jobRunGuid The job run GUID to filter by
   * @return DataFrame containing control job queries
   */
  private def readControlJobQueries(spark: SparkSession, accountId: String, jobRunGuid: String): DataFrame = {
    val reader = new Reader(spark, logger)
    val contractSchema = new ControlJobContractSchema().controlJobContractSchema
    
    val result = reader.readCosmosData(
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
    
    if (result == null) {
      throw new RuntimeException(s"Failed to read control job queries from Cosmos DB for account ID: $accountId")
    }
    
    result.cache()
  }
  
  /**
   * Process query results by applying rules and writing outputs
   *
   * @param queryResults The list of query results to process
   * @param config The JobConfig containing processing parameters
   * @param spark The SparkSession to use
   */
  private def processQueryResults(queryResults: List[QueryResult], config: JobConfig, spark: SparkSession): Unit = {
    val qualityScoreService = new QualityScoreService(spark, logger)
    
    queryResults.zipWithIndex.foreach { case (queryResult, index) =>
      logger.info(s"Processing control job ${index + 1} with ${queryResult.rules.size} rules and control ID: ${queryResult.controlId}")
      processQueryResult(queryResult, qualityScoreService, config, spark)
    }
  }
  
  /**
   * Process a single control job result by applying rules and generating summaries
   *
   * @param queryResult The query result to process
   * @param qualityScoreService The QualityScoreService instance to use
   * @param config The JobConfig containing processing parameters
   * @param spark The SparkSession to use
   */
  private def processQueryResult(
    queryResult: QueryResult,
    qualityScoreService: QualityScoreService, 
    config: JobConfig,
    spark: SparkSession
  ): Unit = {
    for {
      // Apply rules to the data frame
      _ <- CommonUtils.safeExecute(s"process control job result for control ID: ${queryResult.controlId}", logger) {
        logger.info(s"Applying ${queryResult.rules.size} rules to data for control ID: ${queryResult.controlId}")
        
        // Convert Map[String, String] to ControlRule
        val controlRules = queryResult.rules.map { ruleMap =>
          ControlRule(
            ruleId = ruleMap.getOrElse("id", "unknown_rule"),
            ruleName = ruleMap.getOrElse("name", ""),
            description = ruleMap.getOrElse("description", ""),
            condition = ruleMap.getOrElse("condition", ""),
            ruleType = ruleMap.getOrElse("type", "CustomTruth"),
            tags = ruleMap.filter { case (key, _) => 
              key != "id" && key != "name" && key != "description" && key != "condition" && key != "type"
            }
          )
        }
        
        // Apply rules and generate summary
        try {
          // Apply rules to the input DataFrame and generate detailed results with rule_results column
          logger.info(s"Applying rules to input DataFrame and generating detailed results for control ID: ${queryResult.controlId}")
          val dfDetailedRules = qualityScoreService.applyDetailedRules(queryResult.dataFrame, controlRules)
          
          // Generate rule summary directly from the detailed results
          logger.info(s"Generating rule summary for control ID: ${queryResult.controlId}")
          
          import org.apache.spark.sql.functions._
          import spark.implicits._
          
          val dfSummaryRules = dfDetailedRules
            .select(explode(col("rule_results")).as("rule_result"))
            .select(
              col("rule_result.key").as("ruleId"),
              col("rule_result.value").as("result")
            )
            .groupBy("ruleId")
            .agg(
              count(when(col("result") === "PASS", 1)).as("passedCount"),
              count(when(col("result") === "FAIL", 1)).as("failedCount")
            )
            .withColumn(
              "complianceScore",
              when(col("passedCount") + col("failedCount") > 0,
                (col("passedCount") / (col("passedCount") + col("failedCount"))) * 100
              ).otherwise(100.0)
            )
          
          if (dfSummaryRules == null) {
            throw new RuntimeException(s"Failed to generate summary for control ID: ${queryResult.controlId}")
          }
          
          // Create a ControlJobService instance to get the output path
          val controlJobService = new ControlJobService(spark, logger)
          
          // Get the storage path from the service
          controlJobService.getOutputPath(config.accountId, queryResult.controlId, config.jobRunGuid) match {
            case Some(outputPath) =>
              // Extract the rule types map from controlRules
              val ruleTypesMap = controlRules.map(rule => rule.ruleId -> rule.ruleType).toMap
              
              // Transform dfSummaryRules to DataQualityContractSchema before writing
              logger.info(s"Transforming summary rules to DataQualityContractSchema for control ID: ${queryResult.controlId}")
              val transformedDF = transformToControlSummarySchema(dfSummaryRules, config.accountId, queryResult.controlId, config.jobRunGuid, ruleTypesMap, spark)
              
              // Write results to parquet files
              writeResults(dfDetailedRules, transformedDF, outputPath, spark)
            case None =>
              throw new RuntimeException(s"Failed to construct output path for control ID: ${queryResult.controlId}")
          }
        } catch {
          case e: Exception if e.getMessage != null && e.getMessage.contains("Column") && e.getMessage.contains("does not exist") =>
            val errorMsg = s"Column reference error while applying rules for control ID: ${queryResult.controlId}. Error: ${e.getMessage}"
            logger.error(errorMsg)
            throw new RuntimeException(errorMsg, e)
          case e: Exception =>
            throw e
        }
      }
    } yield ()
  }
  
  /**
   * Transform summary rules DataFrame to match DataQualityContractSchema
   *
   * @param dfSummaryRules The summary rules DataFrame
   * @param accountId The account ID
   * @param controlId The control ID
   * @param jobRunGuid The job run GUID
   * @param ruleTypesMap The map of rule IDs to rule types
   * @param spark The SparkSession to use
   * @return DataFrame matching DataQualityContractSchema
   */
  private def transformToControlSummarySchema(
    dfSummaryRules: DataFrame,
    accountId: String,
    controlId: String,
    jobRunGuid: String,
    ruleTypesMap: Map[String, String],
    spark: SparkSession
  ): DataFrame = {
    import spark.implicits._
    
    // Get current timestamp in ISO format
    val currentTimestamp = LocalDateTime.now().format(DateTimeFormatter.ISO_DATE_TIME)
    val eventId = UUID.randomUUID().toString
    
    // Set ID fields according to the specified requirements
    // The controlId comes from processEvaluation method in ControlJobService, 
    // which extracts it from the evaluation row with: evaluation.getAs[String]("controlId")
    val businessDomainId = "___deh_business_domain_id___"  // Always set to this fixed value
    val dataProductId = controlId  // Using the controlId value from the processEvaluation method
    val dataAssetId = jobRunGuid  // Using jobRunGuid from the config
    
    // First, collect the rules data to create the nested structure
    val rulesData = dfSummaryRules.collect().map { row =>
      (
        row.getAs[String]("ruleId"),
        row.getAs[Long]("passedCount"),
        row.getAs[Long]("failedCount"),
        row.getAs[Double]("complianceScore")
      )
    }
    
    // Create a DataQualityContractSchema instance
    val contractSchema = new ControlSummarySchema().controlSummarySchema
    
    // Create a DataFrame with the structure of DataQualityContractSchema
    val dataQualityDF = spark.createDataFrame(
      spark.sparkContext.parallelize(Seq(
        Row(
          // payloadDetails
          Row(
            // id
            Row(businessDomainId, dataProductId, dataAssetId, eventId),
            // jobStatus
            "Succeeded",
            // resultedAt
            currentTimestamp,
            // region
            "uaenorth",
            // facts - create a map of rule IDs to fact objects
            rulesData.map { case (ruleId, passed, failed, _) =>
              ruleId -> Row(passed.toInt, failed.toInt, 0, 0, 0, 0)
            }.toMap,
            // dimensionMapping
            Map[String, String](),
            // domainmodel
            Row(
              // eventSource
              "DataQuality",
              // eventCorrelationId
              jobRunGuid,
              // payloadKind
              "dataQualityFact",
              // operationType
              "upsert",
              // payload
              Row(
                // accountId
                accountId,
                // tenantId
                "0ee21e60-cbf6-4137-af1e-6e2d8b81c72f", // Default tenant ID
                // businessDomainId
                businessDomainId,
                // dataProductId
                dataProductId,
                // dataAssetId
                dataAssetId,
                // jobId
                eventId,
                // puDetail
                "",
                // jobType
                "MDQ",
                // dataQualityFact
                rulesData.map { case (ruleId, passed, failed, score) =>
                  Row(
                    // businessDomainId
                    businessDomainId,
                    // dataProductId
                    dataProductId,
                    // dataAssetId
                    dataAssetId,
                    // jobExecutionId
                    eventId,
                    // ruleId
                    ruleId,
                    // ruleType
                    ruleTypesMap.getOrElse(ruleId, "CustomTruth"),
                    // dimension
                    null.asInstanceOf[String],
                    // result
                    score,
                    // passed
                    passed.toInt,
                    // failed
                    failed.toInt,
                    // miscast
                    0,
                    // empty
                    0,
                    // ignored
                    0,
                    // isAppliedOn
                    "Asset",
                    // ruleOrigin
                    "Asset"
                  )
                },
                // systemData
                Row(
                  // createdAt
                  currentTimestamp,
                  // resultedAt
                  currentTimestamp
                )
              )
            )
            // Removed fields from payloadDetails struct
          ),
          // accountId - keeping original structure for backward compatibility
          accountId,
          // eventId
          eventId,
          // correlationId
          jobRunGuid,
          // preciseTimestamp
          currentTimestamp,
          // payloadKind
          "dataQualityFact",
          // operationType
          "upsert",
          // payloadType
          "inline",
          // puDetail
          "",
          // EventProcessedUtcTime
          currentTimestamp,
          // PartitionId
          2,
          // EventEnqueuedUtcTime
          currentTimestamp,
          // id
          eventId,
          // _rid
          "",
          // _etag
          "",
          // _ts
          System.currentTimeMillis() / 1000
        )
      )),
      contractSchema
    )
    
    dataQualityDF
  }
  
  /**
   * Write detailed and summary results to parquet files
   *
   * @param dfDetailedRules The detailed rules DataFrame
   * @param dfSummaryRules The summary rules DataFrame
   * @param outputPath The output path for writing results
   * @param spark The SparkSession to use
   */
  private def writeResults(
    dfDetailedRules: DataFrame, 
    dfSummaryRules: DataFrame, 
    outputPath: String,
    spark: SparkSession
  ): Unit = {
    // Print the full contents of dfSummaryRules
    logger.info("Showing the complete dfSummaryRules DataFrame:")
    dfSummaryRules.show(false)
    
    // Write detailed rules to the specified path
    logger.info(s"Writing control job detailed rules to parquet at: $outputPath")
    CommonUtils.safeExecute("writing detailed rules to parquet", logger) {
      // Transform rule_results to flattened JSON format before writing
      val qualityScoreService = new QualityScoreService(spark, logger)
      val dfWithFlattenedResults = qualityScoreService.transformRuleResultsToFlattenedJson(dfDetailedRules)
      
      dfWithFlattenedResults
        .withColumnRenamed("rule_results","result")
        .write
        .mode("overwrite")
        .parquet(s"$outputPath")
    }

    // Add id field required by Cosmos DB
    logger.info(s"Adding required 'id' field to dfSummaryRules DataFrame for Cosmos DB")
    val dfSummaryRulesWithIdDF = CommonUtils.safeExecuteWithTry("adding id field", logger) {
      import org.apache.spark.sql.functions._
      
      // Use eventId directly from the root level instead of from payloadDetails
      dfSummaryRules.withColumn("id", col("eventId"))
    } match {
      case Success(df) => df
      case Failure(e) => 
        val errorMsg = s"Failed to add id field: ${e.getMessage}"
        logger.error(errorMsg, e)
        throw new RuntimeException(errorMsg, e)
    }

    // Write complete dfSummaryRules to Cosmos DB
    logger.info(s"Writing all fields of dfSummaryRules to Cosmos DB")
    CommonUtils.safeExecute("writing complete rules summary to Cosmos DB", logger) {
      val cosmosWriter = new CosmosWriter(spark)
      cosmosWriter.writeToCosmosDB(
        df = dfSummaryRulesWithIdDF,
        database = "dgh-DataEstateHealth",
        container = "dataqualityv2fact",
        saveMode = SaveMode.Append,
        entityName = "controlJobRulesSummary"
      )
    }
  }
} 