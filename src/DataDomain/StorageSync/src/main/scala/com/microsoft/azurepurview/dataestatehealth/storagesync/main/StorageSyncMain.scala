package com.microsoft.azurepurview.dataestatehealth.storagesync.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging
import com.microsoft.azurepurview.dataestatehealth.storagesync.auth.TokenManager
import com.microsoft.azurepurview.dataestatehealth.storagesync.common.{CommandLineParser, LakeCopy, LakeCopySpark, LogAnalyticsLogger, Utils}
import org.apache.spark.sql.SparkSession

import java.util.{Date, ResourceBundle}

object StorageSyncMain extends SparkLogging {
  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("storagesync")

    // Retrieve and Log Release Version
    println(
      s"""Release Version:
         |Group ID - ${resourceBundle.getString("groupId")},
         |Artifact ID - ${resourceBundle.getString("artifactId")},
         |Version - ${resourceBundle.getString("version")}""".stripMargin)

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

        TokenManager.initialize(
          spark.conf.get("spark.mitoken.value",""),
          new Date(System.currentTimeMillis + 10000000L)
        )
        // Define the storage endpoint
        val (storageEndpoint, syncRootPath) = Utils.convertUrl(config.SyncRootPath)

        // Set Spark configurations
        spark.conf.set(s"fs.azure.account.auth.type.$storageEndpoint", "Custom")
        spark.conf.set(s"fs.azure.account.oauth.provider.type.$storageEndpoint",
          "com.microsoft.azurepurview.dataestatehealth.storagesync.auth.MITokenProvider")

        val tenantId = spark.conf.get("spark.purview.tenantId", "")
        println(s"PurviewTenantId:$tenantId")

        try {
          println("In StorageSync Main Spark Application!")
          logger.info("Started the StorageSync Main Spark Application!")

          println(
            s"""Received parameters:
               |DEH Tenant Storage - ${config.DEHStorageAccount},
               |Target Root Path - ${config.SyncRootPath},
               |Target Type - ${config.SyncType},
               |Account ID - ${config.AccountId},
               |Job Run Guid - ${config.JobRunGuid}""".stripMargin)

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobStatus = "Started", tenantId = tenantId)

          val lakeCopy = new LakeCopy()
          lakeCopy.processLakehouseCopy(config.DEHStorageAccount.concat("/DomainModel"), syncRootPath.concat("/DomainModel"))

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