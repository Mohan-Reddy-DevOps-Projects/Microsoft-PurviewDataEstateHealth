package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.LogAnalyticsLogger
import com.microsoft.azurepurview.dataestatehealth.commonutils.maintenance.Maintenance
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.CommandLineParser
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import java.util.ResourceBundle

object DimensionalModelMain {
  val logger: Logger = Logger.getLogger(getClass.getName)
  
  private object Config {
    val JOB_NAME = "DimensionalModel"
    val CORRELATION_ID_CONFIG = "spark.correlationId"
    val SWITCH_TO_NEW_CONTROLS_FLOW = "spark.ec.switchToNewControlsFlow"
  }

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("dimensionalmodel")
    var correlationId = ""
    var switchToNewControlsFlow = false
    var spark: SparkSession = null

    // Retrieve and Log Release Version
    println(
      s"""Release Version:
         |Group ID - ${resourceBundle.getString("groupId")},
         |Artifact ID - ${resourceBundle.getString("artifactId")},
         |Version - ${resourceBundle.getString("version")}""".stripMargin)
    logger.setLevel(Level.INFO)
    logger.info(
      s"""Release Version:
         |Group ID - ${resourceBundle.getString("groupId")},
         |Artifact ID - ${resourceBundle.getString("artifactId")},
         |Version - ${resourceBundle.getString("version")}""".stripMargin)

    val parser = new CommandLineParser()
    parser.parse(args) match {
      case Some(config) =>
        logger.setLevel(Level.INFO)
        logger.info("Started the DimensionalModel Main Spark Application!")

        spark = SparkSession.builder
          .appName("DimensionalModelMainSparkApplication")
          .getOrCreate()

        val tenantId = spark.conf.get("spark.purview.tenantId", "")
        println(s"PurviewTenantId:$tenantId")

        try {
          println("In DimensionalModel Main Spark Application!")

          spark.conf.set("spark.cosmos.accountKey", mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.analyticalcosmos.keyname")))

          /*
          if (spark.conf.get("spark.ec.deleteModelFolder", "false").toBoolean) {
            Maintenance.performMaintenance(config.AdlsTargetDirectory.concat("/DimensionalModel"))
          }
          */

          println(
            s"""Received parameters:
               |Target ADLS Path - ${config.AdlsTargetDirectory}
               |Re-processing Threshold (in minutes) - ${config.ReProcessingThresholdInMins}
               |Account ID - ${config.AccountId},
               |Unique JobRunGuid (CorrelationId) - ${config.JobRunGuid}""".stripMargin)

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

          // Processing DimDate Delta Table
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension.DimMain.main(config.AdlsTargetDirectory, config.ReProcessingThresholdInMins, config.AccountId, config.JobRunGuid, spark)
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact.FactMain.main(config.AdlsTargetDirectory, config.ReProcessingThresholdInMins, config.AccountId, config.JobRunGuid, spark)

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
          case e =>
            logger.error(s"Error in DomainModel Main Spark Application: ${e.getMessage}", e)
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
            throw e // Re-throw the exception to ensure the job failure is reported correctly
        } finally {
          if (spark != null) {
            Thread.sleep(10000)
            spark.stop()
          }
        }
      case None =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
    }
  }
}