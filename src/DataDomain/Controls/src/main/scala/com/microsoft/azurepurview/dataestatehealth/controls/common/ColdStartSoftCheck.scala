package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.apache.log4j.Logger
import org.apache.spark.sql.SparkSession

import scala.util.{Failure, Success, Try}

/**
 * Class to handle cold start checks for Cosmos DB connection.
 * 
 * This performs a lightweight validation to ensure the connection is available
 * before proceeding with the main processing.
 *
 * @param spark The SparkSession to use for database operations
 * @param logger The logger instance for output messages
 */
class ColdStartSoftCheck(spark: SparkSession, logger: Logger) {
  
  /**
   * Validates if Cosmos DB connection is working by performing a simple query.
   *
   * This method attempts to read from the specified container with minimal data retrieval
   * to verify that the connection and authentication are working properly.
   *
   * @param containerName The name of the Cosmos DB container to check
   * @param database The Cosmos DB database name (defaults to configuration value)
   * @return Boolean indicating whether the connection check succeeded
   */
  def validateCheckIn(
    containerName: String, 
    database: String = spark.conf.get(CosmosConfig.DATABASE_KEY)
  ): Boolean = {
    if (!CommonUtils.validateStringParam(containerName, "containerName", logger)) {
      throw new IllegalArgumentException("Container name is null or empty")
    }
    
    logger.info(s"Performing cold start check for container: $containerName in database: $database")
    
    val settings = CosmosDbSettings(
      accountEndpoint = spark.conf.get(CosmosConfig.ACCOUNT_ENDPOINT_KEY),
      accountKey = spark.conf.get(CosmosConfig.ACCOUNT_KEY_KEY),
      database = database
    )
    
    // This is a soft check, so we need to handle exceptions differently
    try {
      // Soft Check to Overcome the cold Start Problem
      val df = CosmosConfig.configureReader(spark, containerName, Some(settings)).load()
      
      val isEmpty = df.isEmpty
      logger.info(s"Cold start check for $containerName result: ${if (!isEmpty) "Success" else "Empty dataset"}")
      !isEmpty
    } catch {
      case e: Exception =>
        logger.error(s"Error reading from Cosmos DB container $containerName: ${e.getMessage}", e)
        false
    }
  }
}

