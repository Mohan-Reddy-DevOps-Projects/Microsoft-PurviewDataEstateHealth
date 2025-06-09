package com.microsoft.azurepurview.dataestatehealth.controls.service

import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, Row, SparkSession}
import org.apache.spark.sql.functions._

import scala.collection.mutable.ListBuffer

import com.microsoft.azurepurview.dataestatehealth.commonutils.writer.Reader
import com.microsoft.azurepurview.dataestatehealth.controls.common.CommonUtils
import com.microsoft.azurepurview.dataestatehealth.controls.constants.Constants
import com.microsoft.azurepurview.dataestatehealth.controls.model.{ControlJobContractSchema, QueryResult}

/**
 * Service that handles execution and processing of control jobs.
 * Provides functionality to create temporary tables, process queries, and execute job queries.
 *
 * @param spark The SparkSession to use
 * @param logger Optional logger instance (will create one if not provided)
 */
class ControlJobService(
  spark: SparkSession, 
  private val logger: Logger = Logger.getLogger(classOf[ControlJobService])
) {
 
  import Constants.Columns._
  import Constants.Storage._
  import Constants.FileFormat._
  
  // Cache control job queries for later use in getOutputPath
  private var cachedControlJobQueries: DataFrame = _

  /**
   * Ensures that cached control job queries are available, loading them if needed
   *
   * @param accountId The account ID to query for
   * @return The DataFrame containing control job queries
   */
  private[service] def ensureCachedQueries(accountId: String): DataFrame = {
    if (cachedControlJobQueries == null || cachedControlJobQueries.isEmpty) {
      logger.info(s"Loading control job queries for accountId: $accountId")
      
      // Load the control job queries directly from the source
      val reader = new Reader(spark, logger)
      val contractSchema = new ControlJobContractSchema().controlJobContractSchema
      
      val queries = reader.readCosmosData(
        contractSchema, 
        "",
        accountId,
        Constants.Cosmos.CONTAINER, 
        "", 
        "", 
        Constants.Cosmos.DATABASE
      )
      
      if (queries == null) {
        throw new RuntimeException(s"Failed to read control job queries from Cosmos DB for account ID: $accountId")
      }
      
      // Cache the queries for future use
      cachedControlJobQueries = queries
    }
    
    cachedControlJobQueries
  }

  /**
   * Creates temporary tables for each storage endpoint in the input DataFrame
   *
   * @param processedInputsDF DataFrame containing storage endpoint information
   * @return Number of successfully created temporary tables
   */
  def createTempTables(processedInputsDF: DataFrame): Int = {
    // Process each storage endpoint
    val storageEndpoints = processedInputsDF
      .select("constructedStorageEndpoint")
      .distinct()
      .collect()
      .map(_.getString(0))

    logger.info(s"Found ${storageEndpoints.length} distinct storage endpoints to process")
    
    var successfulCreations = 0

    storageEndpoints.foreach { storageEndpoint =>
      val domainModelPath = storageEndpoint
      logger.info(s"Processing domain model path: $domainModelPath")

      CommonUtils.safeExecute(s"create temp table for $domainModelPath", logger) {
        val tableName = extractTableName(domainModelPath)
        createTempTable(domainModelPath, tableName)
        successfulCreations += 1
      }
    }
    
    successfulCreations
  }
  
  /**
   * Extracts a table name from a storage path
   *
   * @param path The storage path
   * @return A sanitized table name
   */
  private def extractTableName(path: String): String = {
    val rawName = path.split("/").last
    s"${rawName.replaceAll("[^a-zA-Z0-9]", "_")}"
  }
  
  /**
   * Creates a temporary table from a storage path
   *
   * @param path The storage path
   * @param tableName The table name to use
   */
  private def createTempTable(path: String, tableName: String): Unit = {
    logger.info(s"Creating temporary table '$tableName' from path: $path")
    
    val df = spark.read.format(DELTA).load(path).cache()
    df.createOrReplaceTempView(tableName)
    
    logger.info(s"Created temporary table '$tableName'")
  }
  
  /**
   * Processes job queries from a DataFrame of configurations
   *
   * @param controlJobQueries DataFrame containing control job queries
   * @return List of QueryResult objects
   */
  def processJobQueries(controlJobQueries: DataFrame): List[QueryResult] = {
    val resultList = ListBuffer.empty[QueryResult]
    
    // Cache the control job queries for later use in getOutputPath
    this.cachedControlJobQueries = controlJobQueries
    
    // Extract storage endpoint from inputs
    val inputsDF = controlJobQueries
      .select(
        col("id"),
        explode(col("inputs")).as("input")
      )
      .select(
        col("id"),
        col("input.typeProperties.datasourceFQN").as("datasourceFQN"),
        col("input.typeProperties.fileSystem").as("fileSystem"),
        col("input.typeProperties.folderPath").as("folderPath")
      )

    // Construct abfss:// storage endpoint
    val processedInputsDF = inputsDF.withColumn(
      "constructedStorageEndpoint",
      concat(
        lit(ABFSS_PREFIX),
        col("fileSystem"),
        lit("@"),
        regexp_extract(col("datasourceFQN"), DATASOURCE_HOST_PATTERN, 1),
        lit("/"),
        col("folderPath")
      )
    )

    // Check if we have any inputs
    if (processedInputsDF.isEmpty) {
      throw new RuntimeException("No inputs data found in control job queries")
    }

    // Create temporary tables before executing queries
    val tablesCreated = createTempTables(processedInputsDF)
    logger.info(s"Created $tablesCreated temporary tables from storage endpoints")

    // Process each row and its evaluations
    val rows = controlJobQueries.collect()
    
    rows.foreach { row =>
      // Get the evaluations array from the row
      val evaluations = row.getAs[Seq[Row]]("evaluations")
      if (evaluations != null && evaluations.nonEmpty) {
        evaluations.foreach { evaluation =>
          CommonUtils.safeExecute(s"processing job query for evaluation", logger) {
            processEvaluation(evaluation).foreach(resultList += _)
          }
        }
      } else {
        logger.warn(s"No evaluations found for row with ID: ${row.getAs[String]("id")}")
      }
    }
    
    if (resultList.isEmpty) {
      throw new RuntimeException("No query results were processed successfully")
    }
    
    // Return the list of QueryResult objects
    resultList.toList
  }
  
  /**
   * Process a single evaluation row and return a QueryResult
   *
   * @param evaluation The evaluation Row to process
   * @return An Option containing the QueryResult if successful, None otherwise
   */
  private def processEvaluation(evaluation: Row): Option[QueryResult] = {
    try {
      val controlId = evaluation.getAs[String]("controlId")
      val query = evaluation.getAs[String]("query")
      
      // Extract and validate rules
      val rules = evaluation.getAs[Seq[Row]]("rules")
      if (rules == null || rules.isEmpty) {
        throw new RuntimeException(s"No rules found for control ID: $controlId")
      }
      
      val rulesList = rules.map { rule =>
        rule.getValuesMap[String](rule.schema.fieldNames)
      }
      
      logger.info(s"Processing job query for control ID: $controlId with ${rulesList.size} rules")
      
      // Execute the query
      val queryDataFrame = executeQuery(query, controlId)
      
      // Return the result
      Some(QueryResult(queryDataFrame, rulesList, controlId))
    } catch {
      case e: Exception =>
        logger.error(s"Error processing evaluation: ${e.getMessage}", e)
        throw new RuntimeException(s"Error processing evaluation: ${e.getMessage}", e)
    }
  }
  
  /**
   * Executes a SQL query
   *
   * @param query The SQL query to execute
   * @param controlId The control ID for logging
   * @return The DataFrame result of the query
   */
  private def executeQuery(query: String, controlId: String): DataFrame = {
    logger.info(s"Executing query for control ID: $controlId")
    val result = spark.sql(query)
    logger.info(s"Successfully executed query for control ID: $controlId")
    result
  }
  
  /**
   * Retrieves the storage account details and constructs the output path
   *
   * @param accountId The account ID to query for storage information
   * @param controlId The control ID for the path
   * @param jobRunGuid The job run GUID for the path
   * @return Option containing the full output path if successful
   */
  def getOutputPath(accountId: String, controlId: String, jobRunGuid: String): Option[String] = {
    try {
      // Ensure we have the cached control job queries
      val queries = ensureCachedQueries(accountId)
      
      // Check if 'inputs' column exists
      if (!queries.schema.fieldNames.contains("inputs")) {
        throw new RuntimeException(s"Control job queries does not contain 'inputs' column. Schema: ${queries.schema.treeString}")
      }
      
      // Extract storage endpoint from inputs, similar to processJobQueries
      val inputsDF = queries
        .select(explode(col("inputs")).as("input"))
        .select(
          col("input.typeProperties.datasourceFQN").as("datasourceFQN"),
          col("input.typeProperties.fileSystem").as("fileSystem")
        )
      
      if (inputsDF.isEmpty) {
        throw new RuntimeException("No storage account information found in inputs")
      }
      
      // Construct storage endpoint exactly like in processJobQueries
      val processedInputsDF = inputsDF.withColumn(
        "constructedStorageEndpoint",
        concat(
          lit(ABFSS_PREFIX),
          col("fileSystem"),
          lit("@"),
          regexp_extract(col("datasourceFQN"), DATASOURCE_HOST_PATTERN, 1),
          lit("/")
        )
      )
      
      // Get the first row's storage endpoint
      val storageEndpoint = processedInputsDF.select("constructedStorageEndpoint").first().getString(0)
      
      // Construct the folder path structure
      val folderPath = CommonUtils.constructPath(
        "all-errors",
        "businessDomain=___deh_business_domain_id___",
        s"dataProduct=$controlId",
        s"dataAsset=$jobRunGuid",
        s"observation=$jobRunGuid"
      )
      
      val outputPath = s"$storageEndpoint$folderPath"
      logger.info(s"Constructed output path: $outputPath")
      Some(outputPath)
    } catch {
      case e: Exception =>
        logger.error(s"Error constructing output path: ${e.getMessage}", e)
        throw new RuntimeException(s"Error constructing output path: ${e.getMessage}", e)
    }
  }
} 