package com.microsoft.azurepurview.dataestatehealth.domainmodel.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.LogAnalyticsLogger
import com.microsoft.azurepurview.dataestatehealth.commonutils.maintenance.Maintenance
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.CommandLineParser
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import java.util.ResourceBundle

object DomainModelMain {
  val logger: Logger = Logger.getLogger(getClass.getName)
  
  private object Config {
    val JOB_NAME = "DomainModel"
    val CORRELATION_ID_CONFIG = "spark.correlationId"
    val SWITCH_TO_NEW_CONTROLS_FLOW = "spark.ec.switchToNewControlsFlow"
  }

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("domainmodel")
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
        logger.info("Started the DomainModel Main Spark Application!")

        spark = SparkSession.builder
          .appName("DomainModelMainSparkApplication")
          .getOrCreate()

        val tenantId = spark.conf.get("spark.purview.tenantId", "")
        println(s"PurviewTenantId:$tenantId")

        try {
          println("In DomainModel Main Spark Application!")

          spark.conf.set("spark.cosmos.accountKey", mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.analyticalcosmos.keyname")))

          if (spark.conf.get("spark.ec.deleteModelFolder", "false").toBoolean){
            Maintenance.performMaintenance(config.AdlsTargetDirectory)
          }

          println(
            s"""Received parameters:
               |Target ADLS Path - ${config.AdlsTargetDirectory},
               |Account ID - ${config.AccountId},
               |Refresh Type - ${config.RefreshType},
               |Re-processing Threshold (in minutes) - ${config.ReProcessingThresholdInMins},
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

          // Processing BusinessDomain Delta Table
          com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain.BusinessDomainMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct.DataProductMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm.GlossaryTermMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship.RelationshipMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset.DataAssetMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset.AccessPolicySetMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription.SubscriptionMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality.DataQualityMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.objective.ObjectiveMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.action.HealthActionMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.cde.CDEMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)

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
      case _ =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
    }
  }
}