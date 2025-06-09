package com.microsoft.azurepurview.dataestatehealth.commonutils.writer.cosmosdb

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging
import org.apache.spark.sql.{DataFrame, SaveMode, SparkSession}

/**
 * CosmosWriter class provides utilities to write data to Azure Cosmos DB.
 *
 * @param spark The SparkSession to use for the operations
 */
class CosmosWriter(spark: SparkSession) extends SparkLogging {

  /**
   * Writes data to a Cosmos DB container.
   *
   * @param df The DataFrame to write to Cosmos DB
   * @param database The Cosmos DB database name
   * @param container The Cosmos DB container name
   * @param saveMode The save mode to use (Default: Append)
   * @param entityName Optional entity name for logging purposes
   * @param additionalOptions Optional additional configuration options for the Cosmos DB connector
   * @return Unit
   */
  def writeToCosmosDB(
    df: DataFrame,
    database: String,
    container: String,
    saveMode: SaveMode = SaveMode.Append,
    entityName: String = "",
    additionalOptions: Map[String, String] = Map.empty
  ): Unit = {
    try {
      logger.info(s"Starting to write data to Cosmos DB. Database: $database, Container: $container, Entity: $entityName")
      
      // Validate inputs
      if (df == null) {
        throw new IllegalArgumentException("DataFrame cannot be null")
      }
      
      if (database.isEmpty || container.isEmpty) {
        throw new IllegalArgumentException("Database and container names cannot be empty")
      }
      
      // Get Cosmos DB connection information from Spark configuration
      val accountEndpoint = spark.conf.get("spark.cosmos.accountEndpoint")
      val accountKey = spark.conf.get("spark.cosmos.accountKey")
      
      // Basic options required for Cosmos DB connection
      val baseOptions = Map(
        "spark.cosmos.accountEndpoint" -> accountEndpoint,
        "spark.cosmos.accountKey" -> accountKey,
        "spark.cosmos.database" -> database,
        "spark.cosmos.container" -> container
      )
      
      // Merge base options with any additional options
      val options = baseOptions ++ additionalOptions
      
      // Write the DataFrame to Cosmos DB
      val writer = df.write.format("cosmos.oltp")
      
      // Apply all options
      options.foreach { case (key, value) =>
        writer.option(key, value)
      }
      
      // Set the save mode and execute the write
      writer.mode(saveMode).save()
      
      logger.info(s"Successfully wrote data to Cosmos DB. Database: $database, Container: $container, Entity: $entityName")
    } catch {
      case e: IllegalArgumentException =>
        val errorMsg = s"Invalid argument while writing to Cosmos DB: ${e.getMessage}"
        logger.error(errorMsg, e)
        throw e
      case e: Exception =>
        val entityInfo = if (entityName.nonEmpty) s" Asset $entityName" else ""
        val errorMsg = s"Error writing to Cosmos DB container '$container'$entityInfo: ${e.getMessage}"
        logger.error(errorMsg, e)
        println(errorMsg)
        throw e
    }
  }
  
  /**
   * Simplified method to write data to a Cosmos DB container using default configuration.
   * This method is compatible with the reference function's signature.
   *
   * @param df The DataFrame to write to Cosmos DB
   * @param entityName The entity name for logging purposes
   * @param database Optional Cosmos DB database name (default: from spark.conf)
   * @param container Optional Cosmos DB container name (default: "dehsentinel")
   * @return Unit
   */
  def writeCosmosData(
    df: DataFrame, 
    entityName: String,
    database: String = spark.conf.get("spark.cosmos.database"),
    container: String = "dehsentinel"
  ): Unit = {
    writeToCosmosDB(
      df = df,
      database = database,
      container = container,
      saveMode = SaveMode.Append,
      entityName = entityName
    )
  }
} 