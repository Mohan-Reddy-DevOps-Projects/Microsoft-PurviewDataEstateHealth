package com.microsoft.azurepurview.dataestatehealth.storagesync.main

import com.microsoft.azurepurview.dataestatehealth.storagesync.auth.TokenManager
import com.microsoft.azurepurview.dataestatehealth.storagesync.common.{CommandLineParser, LakeCopy, LakeCopySpark, LogAnalyticsLogger, Utils}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import java.util.{Date, ResourceBundle}

object StorageSyncMain {
  val logger : Logger = Logger.getLogger(getClass.getName)
  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("storagesync")

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
          .appName("StorageSyncMainSparkApplication")
          .getOrCreate()

        val tenantId = spark.conf.get("spark.purview.tenantId", "")

        try {

          println("In StorageSync Main Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the StorageSync Main Spark Application!")

          println(
            s"""Received parameters:
               |DEH Tenant Storage - ${config.DEHStorageAccount},
               |Target Root Path - ${config.SyncRootPath},
               |Target Type - ${config.SyncType},
               |Account ID - ${config.AccountId},
               |Job Run Guid - ${config.JobRunGuid}""".stripMargin)

          var fabricSyncRootPath = config.SyncRootPath
          if (config.SyncRootPath.startsWith("http") )
          {
            println("converting http path to abfss..")
            println(fabricSyncRootPath)
            fabricSyncRootPath = Utils.convertUrl(config.SyncRootPath)
            println(fabricSyncRootPath)
          }

          TokenManager.initialize(
            spark.conf.get("spark.mitoken.value"),
            new Date(System.currentTimeMillis + 10000000L)
          )

          // Define the storage endpoint
          val storageEndpoint = Utils.getStorageEndpoint(storageType = config.SyncType)

          // Set Spark configurations
          spark.conf.set(s"fs.azure.account.auth.type.$storageEndpoint", "Custom")
          spark.conf.set(s"fs.azure.account.oauth.provider.type.$storageEndpoint", "com.microsoft.azurepurview.dataestatehealth.storagesync.auth.MITokenProvider")

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobStatus = "Started", tenantId = tenantId)

          val lakeCopy = new LakeCopy(logger)
          lakeCopy.processLakehouseCopy(config.DEHStorageAccount.concat("/DomainModel"), fabricSyncRootPath.concat("/DomainModel"))
          lakeCopy.processLakehouseCopy(config.DEHStorageAccount.concat("/DimensionalModel"), fabricSyncRootPath.concat("/DimensionalModel"))

          /*
                    val lakeCopySpark = new LakeCopySpark(spark)
                    lakeCopySpark.processFolders(config.DEHStorageAccount.concat("/DomainModel"), fabricSyncRootPath.concat("/DomainModel"))
                    lakeCopySpark.processFolders(config.DEHStorageAccount.concat("/DimensionalModel"),fabricSyncRootPath.concat("/DimensionalModel"))

           */
        }
        catch
        {
          case e: Exception =>
            println(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
        } finally {
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            if (Thread.currentThread.isInterrupted) "Cancelled" else "Completed", tenantId = tenantId)
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