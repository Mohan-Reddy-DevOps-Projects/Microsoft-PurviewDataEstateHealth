package com.microsoft.azurepurview.dataestatehealth.dehfabricsync.main

import com.microsoft.azurepurview.dataestatehealth.dehfabricsync.auth.TokenManager
import com.microsoft.azurepurview.dataestatehealth.dehfabricsync.common.{CommandLineParser, LakehouseCopy, LogAnalyticsLogger}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import java.util.{Date, ResourceBundle}

object DEHFabricSyncMain {
  val logger : Logger = Logger.getLogger(getClass.getName)
  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("dehfabricsync")

    // Retrieve and Log Release Version
    println(
      s"""Release Version:
         |Group ID - ${resourceBundle.getString("groupId")},
         |Artifact ID - ${resourceBundle.getString("artifactId")},
         |Version - ${resourceBundle.getString("version")}""".stripMargin)
    logger.setLevel(Level.INFO)
    logger.info(s"""Release Version:
                   |Group ID - ${resourceBundle.getString("groupId")},
                   |Artifact ID - ${resourceBundle.getString("artifactId")},
                   |Version - ${resourceBundle.getString("version")}""".stripMargin)

    val parser = new CommandLineParser()
    parser.parse(args) match {
      case Some(config) =>
        val spark = SparkSession.builder
          .appName("DEHFabricSyncMainSparkApplication")
          .getOrCreate()

        try {
          println("In DEHFabricSync Main Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the DEHFabricSync Main Spark Application!")


          println(
            s"""Received parameters:
               |DEH Tenant Storage - ${config.DEHStorageAccount},
               |Target OneLake Fabric Root Path - ${config.FabricSyncRootPath},
               |Account ID - ${config.AccountId},
               |Process Domain Model - ${config.ProcessDomainModel}
               |Process Dimensional Model - ${config.ProcessDimensionalModel}
               |Job Run Guid - ${config.JobRunGuid}""".stripMargin)

          // Initialize TokenManager with initial token and expiry time
          TokenManager.initialize(
            spark.conf.get("spark.mitoken.value"),
            new Date(System.currentTimeMillis() + 240 * 1000L)
          )

          // Define the storage endpoint
          val storageEndpoint = "onelake.dfs.fabric.microsoft.com"

          // Set Spark configurations
          spark.conf.set(s"fs.azure.account.auth.type.$storageEndpoint", "Custom")
          spark.conf.set(s"fs.azure.account.oauth.provider.type.$storageEndpoint", "com.microsoft.azurepurview.dataestatehealth.dehfabricsync.auth.MITokenProvider")
          // Initialize LogAnalyticsConfig with Spark session

          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobStatus = "Started")

          if (config.ProcessDomainModel){
            val lakehouseDomainModelSync = new LakehouseCopy(spark,logger)
            lakehouseDomainModelSync.processLakehouseCopy(config.DEHStorageAccount.concat("/DomainModel"),config.FabricSyncRootPath)
          }
          if (config.ProcessDimensionalModel){
            val lakehouseDimensionalModelSync = new LakehouseCopy(spark,logger)
            lakehouseDimensionalModelSync.processLakehouseCopy(config.DEHStorageAccount.concat("/DimensionalModel"),config.FabricSyncRootPath)
          }
        }
        catch
        {
          case e: Exception =>
            println(s"Error In DEHFabricSync Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In DEHFabricSync Main Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In DEHFabricSync Main Spark Application!: ${e.getMessage}")
        } finally {
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            if (Thread.currentThread.isInterrupted) "Cancelled" else "Completed")
          if (spark != null) {
            Thread.sleep(10000)
            spark.stop()
          }
        }
      case _ =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
    }
  }
}