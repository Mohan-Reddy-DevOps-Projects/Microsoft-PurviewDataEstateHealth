package com.microsoft.azurepurview.dataestatehealth.storagesync.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.{LogAnalyticsLogger, SparkLogging}
import com.microsoft.azurepurview.dataestatehealth.storagesync.auth.TokenManager
import com.microsoft.azurepurview.dataestatehealth.storagesync.common.{CommandLineParser, LakeCopy, Utils}
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

    logger.info(
      s"""Release Version:
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
          spark.conf.get("spark.mitoken.value", ""),
          new Date(System.currentTimeMillis + 10000000L)
        )

        val tenantId = spark.conf.get("spark.purview.tenantId", "")

        try {
          // Define the storage endpoint
          val (storageEndpoint, syncRootPath) = Utils.convertUrl(config.SyncRootPath)

          // Set Spark configurations
          spark.conf.set(s"fs.azure.account.auth.type.$storageEndpoint", "Custom")
          spark.conf.set(s"fs.azure.account.oauth.provider.type.$storageEndpoint",
            "com.microsoft.azurepurview.dataestatehealth.storagesync.auth.MITokenProvider")

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
            jobName = "StorageSync", jobStatus = JobStatus.Started.toString, tenantId = tenantId)

          val lakeCopy = new LakeCopy()
          lakeCopy.processLakehouseCopy(config.DEHStorageAccount.concat("/DomainModel"), syncRootPath.concat("/DomainModel"))

          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobName = "StorageSync", JobStatus.Completed.toString, tenantId = tenantId)
        }
        catch {
          case e: Exception =>
            println(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
            LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
              jobName = "StorageSync", JobStatus.Failed.toString, tenantId = tenantId, e.toString)
            throw new IllegalArgumentException(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
        } finally {
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