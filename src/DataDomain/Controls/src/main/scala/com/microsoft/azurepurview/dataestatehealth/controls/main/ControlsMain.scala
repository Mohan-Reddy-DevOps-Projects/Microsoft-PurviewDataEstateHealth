package com.microsoft.azurepurview.dataestatehealth.controls.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.controls.common.{CommandLineParser, MainConfig}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import scala.util.Try

/**
 * Main entry point for Controls processing.
 * 
 * This class handles command line parsing, Spark session management,
 * and orchestrates the execution of Control Jobs.
 */
object ControlsMain {
  private val logger: Logger = Logger.getLogger(getClass.getName)
  
  /**
   * Constants used throughout the Controls Main application
   */
  private object Config {
    val JOB_NAME = "Controls"
    val LOG_WAIT_MS = 5000
    val APP_NAME = "ControlsMainSparkApplication"
    val TENANT_ID_CONFIG = "spark.purview.tenantId"
    val KEY_VAULT_NAME_CONFIG = "spark.keyvault.name"
    val COSMOS_KEY_NAME_CONFIG = "spark.analyticalcosmos.keyname"
    val COSMOS_ACCOUNT_KEY_CONFIG = "spark.cosmos.accountKey"
  }

  /**
   * Main entry point for the application
   *
   * @param args Command line arguments
   */
  def main(args: Array[String]): Unit = {
    var jobStatus = JobStatus.Failed
    
    try {
      logger.setLevel(Level.INFO)
      logger.info("Starting Controls Main application")
      
      // Parse command line arguments
      val config = CommandLineParser.parseArgs(args)
      
      // Create Spark session
      val spark = createSparkSession(config)
      
      // Process Control Jobs
      ControlJobExecutor.main(
        Array(
          config.adlsTargetDirectory,
          config.accountId,
          config.refreshType,
          config.jobRunGuid
        ),
        spark,
        config.reProcessingThresholdInMins
      )
      
      jobStatus = JobStatus.Completed
      logger.info("Controls Main application completed successfully")
      
    } catch {
      case e: Exception =>
        logger.error(s"Error in Controls Main application: ${e.getMessage}", e)
        jobStatus = JobStatus.Failed
    } finally {
      // Log final job status
      logJobStatus(jobStatus)
    }
  }
  
  /**
   * Creates a SparkSession for the application
   *
   * @param config The application configuration
   * @return A configured SparkSession
   */
  private def createSparkSession(config: MainConfig): SparkSession = {
    logger.info("Creating Spark session")
    
    val builder = SparkSession.builder()
      .appName(Config.APP_NAME)
    
    // Set Tenant ID if available
    if (config.tenantId.nonEmpty) {
      logger.info(s"Setting tenant ID: ${config.tenantId}")
      builder.config(Config.TENANT_ID_CONFIG, config.tenantId)
    }
    
    // Create session and set additional configurations
    val spark = builder.getOrCreate()
    
    // Set Parquet legacy mode for backward compatibility
    spark.conf.set("spark.sql.legacy.parquet.datetimeRebaseModeInWrite", "LEGACY")
    spark.conf.set("spark.sql.legacy.parquet.datetimeRebaseModeInRead", "LEGACY")
    
    // Set Cosmos DB key from KeyVault 
    // In Synapse or Databricks environments, mssparkutils is directly available
    try {
      // This needs to be directly set for Cosmos DB connectivity to work
      // Import for com.microsoft.spark.utils.MSSparkUtils should be automatically
      // available in Synapse environment
      
      // Set key vault name and cosmos key name
      val keyVaultName = "deh-key-vault" // Default value
      val cosmosKeyName = "cosmos-db-key" // Default value
      
      // Set custom values if available in Spark configuration
      spark.conf.getOption(Config.KEY_VAULT_NAME_CONFIG).foreach { kv =>
        logger.info(s"Using configured KeyVault: $kv")
        spark.conf.set(Config.KEY_VAULT_NAME_CONFIG, kv)
      }
      
      spark.conf.getOption(Config.COSMOS_KEY_NAME_CONFIG).foreach { ck =>
        logger.info(s"Using configured Cosmos key name: $ck")
        spark.conf.set(Config.COSMOS_KEY_NAME_CONFIG, ck)
      }
      
      // Hard-code for testing if needed (remove in production)
      // spark.conf.set(Config.COSMOS_ACCOUNT_KEY_CONFIG, "your-cosmos-key-here")
      
      // The original approach from the old code - this is what we need to use
      spark.conf.set(Config.COSMOS_ACCOUNT_KEY_CONFIG,
        mssparkutils.credentials.getSecret(
          spark.conf.get(Config.KEY_VAULT_NAME_CONFIG), 
          spark.conf.get(Config.COSMOS_KEY_NAME_CONFIG)))
      
      logger.info("Successfully set Cosmos DB key from KeyVault")
    } catch {
      case e: Exception =>
        logger.error(s"Failed to set Cosmos DB key: ${e.getMessage}", e)
        // For testing only - REMOVE IN PRODUCTION:
        // spark.conf.set(Config.COSMOS_ACCOUNT_KEY_CONFIG, "dummy-key")
    }
    
    logger.info("Spark session created successfully")
    spark
  }
  
  /**
   * Logs the final job status
   *
   * @param status The job status to log
   */
  private def logJobStatus(status: JobStatus.JobStatus): Unit = {
    logger.info(s"Final job status: $status")
    
    // Wait briefly to ensure logs are flushed
    Try {
      Thread.sleep(Config.LOG_WAIT_MS)
    }
  }
}
