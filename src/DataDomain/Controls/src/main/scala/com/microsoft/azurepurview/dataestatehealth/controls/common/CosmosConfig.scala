package com.microsoft.azurepurview.dataestatehealth.controls.common

import org.apache.spark.sql.SparkSession

/**
 * Configuration for Cosmos DB access.
 * 
 * This class centralizes all configuration related to Cosmos DB connections
 * and provides a clean interface for retrieving configuration values.
 */
object CosmosConfig {
  // Configuration keys
  val ACCOUNT_ENDPOINT_KEY = "spark.cosmos.accountEndpoint"
  val ACCOUNT_KEY_KEY = "spark.cosmos.accountKey" 
  val DATABASE_KEY = "spark.cosmos.database"
  val CONTAINER_KEY = "spark.cosmos.container"
  
  // Format specifier
  val FORMAT = "cosmos.olap"
  
  /**
   * Retrieves Cosmos DB configuration from a SparkSession
   *
   * @param spark The SparkSession containing configuration
   * @return A CosmosDbSettings object with all required settings
   */
  def getSettings(spark: SparkSession): CosmosDbSettings = {
    CosmosDbSettings(
      accountEndpoint = spark.conf.get(ACCOUNT_ENDPOINT_KEY),
      accountKey = spark.conf.get(ACCOUNT_KEY_KEY),
      database = spark.conf.get(DATABASE_KEY)
    )
  }
  
  /**
   * Applies Cosmos DB settings to a read operation
   *
   * @param spark The SparkSession to read with
   * @param containerName The container to read from
   * @param settings Optional custom settings (uses spark config if not provided)
   * @return A configured DataFrameReader
   */
  def configureReader(
    spark: SparkSession, 
    containerName: String,
    settings: Option[CosmosDbSettings] = None
  ) = {
    val dbSettings = settings.getOrElse(getSettings(spark))
    
    spark.read.format(FORMAT)
      .option(ACCOUNT_ENDPOINT_KEY, dbSettings.accountEndpoint)
      .option(DATABASE_KEY, dbSettings.database)
      .option(CONTAINER_KEY, containerName)
      .option(ACCOUNT_KEY_KEY, dbSettings.accountKey)
  }
}

/**
 * Settings for Cosmos DB connections
 *
 * @param accountEndpoint The Cosmos DB account endpoint
 * @param accountKey The Cosmos DB account key
 * @param database The Cosmos DB database name
 */
case class CosmosDbSettings(
  accountEndpoint: String,
  accountKey: String,
  database: String
) 