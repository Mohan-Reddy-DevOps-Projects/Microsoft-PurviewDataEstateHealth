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

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("domainmodel")

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

        val spark = SparkSession.builder
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

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobName = "DomainModel", jobStatus = JobStatus.Started.toString, tenantId = tenantId)

          // Processing BusinessDomain Delta Table
          com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain.BusinessDomainMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct.DataProductMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm.GlossaryTermMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship.RelationshipMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset.DataAssetMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset.AccessPolicySetMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription.SubscriptionMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality.DataQualityMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          // com.microsoft.azurepurview.dataestatehealth.domainmodel.objective.ObjectiveMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          com.microsoft.azurepurview.dataestatehealth.domainmodel.action.ActionMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)
          // com.microsoft.azurepurview.dataestatehealth.domainmodel.cde.CDEMain.main(Array(config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid), spark, config.ReProcessingThresholdInMins)

          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobName = "DomainModel", JobStatus.Completed.toString, tenantId = tenantId)

        }
        catch {
          case e =>
            logger.error(s"Error in DomainModel Main Spark Application: ${e.getMessage}", e)
            LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
              jobName = "DomainModel", JobStatus.Failed.toString, tenantId = tenantId, e.toString)
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