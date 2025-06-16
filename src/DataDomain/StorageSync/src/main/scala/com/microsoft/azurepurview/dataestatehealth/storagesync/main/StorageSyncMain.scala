package com.microsoft.azurepurview.dataestatehealth.storagesync.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.{LogAnalyticsLogger, SparkLogging}
import com.microsoft.azurepurview.dataestatehealth.storagesync.auth.TokenManager
import com.microsoft.azurepurview.dataestatehealth.storagesync.common.{CommandLineParser, LakeCopy, Utils}
import org.apache.spark.sql.SparkSession

import java.util.{Date, ResourceBundle}

object StorageSyncMain extends SparkLogging {
  private object Config {
    val JOB_NAME = "StorageSync"
    val CORRELATION_ID_CONFIG = "spark.correlationId"
    val SWITCH_TO_NEW_CONTROLS_FLOW = "spark.ec.switchToNewControlsFlow"
  }

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("storagesync")
    var correlationId = ""
    var switchToNewControlsFlow = false
    var spark: SparkSession = null

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
        spark = SparkSession.builder
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

          // Get correlation ID and switch value
          correlationId = spark.conf.get(Config.CORRELATION_ID_CONFIG, "")
          switchToNewControlsFlow = spark.conf.get(Config.SWITCH_TO_NEW_CONTROLS_FLOW, "false").toBoolean

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          
          // Log job status with correlation ID if switch is enabled
          if (switchToNewControlsFlow) {
            LogAnalyticsLogger.checkpointJobStatus(
              accountId = config.AccountId,
              jobRunGuid = config.JobRunGuid,
              jobName = Config.JOB_NAME,
              jobStatus = JobStatus.Started.toString,
              tenantId = tenantId,
              correlationId = correlationId
            )
          } else {
            LogAnalyticsLogger.checkpointJobStatus(
              accountId = config.AccountId,
              jobRunGuid = config.JobRunGuid,
              jobName = Config.JOB_NAME,
              jobStatus = JobStatus.Started.toString,
              tenantId = tenantId
            )
          }

          val lakeCopy = new LakeCopy()
          lakeCopy.processLakehouseCopy(config.DEHStorageAccount.concat("/DomainModel"), syncRootPath.concat("/DomainModel"))

          // Log completion with correlation ID if switch is enabled
          if (switchToNewControlsFlow) {
            LogAnalyticsLogger.checkpointJobStatus(
              accountId = config.AccountId,
              jobRunGuid = config.JobRunGuid,
              jobName = Config.JOB_NAME,
              jobStatus = JobStatus.Completed.toString,
              tenantId = tenantId,
              correlationId = correlationId
            )
          } else {
            LogAnalyticsLogger.checkpointJobStatus(
              accountId = config.AccountId,
              jobRunGuid = config.JobRunGuid,
              jobName = Config.JOB_NAME,
              jobStatus = JobStatus.Completed.toString,
              tenantId = tenantId
            )
          }
        }
        catch {
          case e: Exception =>
            println(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In StorageSync Main Spark Application!: ${e.getMessage}")
            // Log failure with correlation ID if switch is enabled
            if (config != null && spark != null) {
              if (switchToNewControlsFlow) {
                LogAnalyticsLogger.checkpointJobStatus(
                  accountId = config.AccountId,
                  jobRunGuid = config.JobRunGuid,
                  jobName = Config.JOB_NAME,
                  jobStatus = JobStatus.Failed.toString,
                  tenantId = tenantId,
                  correlationId = correlationId,
                  errorMessage = e.toString
                )
              } else {
                LogAnalyticsLogger.checkpointJobStatus(
                  accountId = config.AccountId,
                  jobRunGuid = config.JobRunGuid,
                  jobName = Config.JOB_NAME,
                  jobStatus = JobStatus.Failed.toString,
                  tenantId = tenantId,
                  errorMessage = e.toString
                )
              }
            }
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