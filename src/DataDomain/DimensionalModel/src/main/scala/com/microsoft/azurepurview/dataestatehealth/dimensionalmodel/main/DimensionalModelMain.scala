package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.main

import com.microsoft.azurepurview.dataestatehealth.commonutils.common.JobStatus
import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.LogAnalyticsLogger
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.CommandLineParser
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import java.util.ResourceBundle

object DimensionalModelMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def performMaintenance(accountId: String, adlsDir: String): Unit = {
    def fileExists(path: String): Boolean = {
      try {
        mssparkutils.fs.ls(path).nonEmpty
      } catch {
        case _: Exception => false
      }
    }

    val basePath = adlsDir
    val filePath = s"$basePath/Maintenance/OneTimeCleanup/DimensionalModel.txt"

    if (fileExists(filePath)) {
      println("SKIP: Delete root directory & full load.")
    } else {
      if (fileExists(adlsDir.concat("/DimensionalModel"))) {
        println("Delete root directory & full load.")
        mssparkutils.fs.rm(adlsDir.concat("/DimensionalModel"), true)
      } else {
        println("No root directory.")
      }
      mssparkutils.fs.put(filePath, accountId)
    }
  }

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("dimensionalmodel")
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

        val spark = SparkSession.builder
          .appName("DimensionalModelMainSparkApplication")
          .getOrCreate()

        val tenantId = spark.conf.get("spark.purview.tenantId", "")
        println(s"PurviewTenantId:$tenantId")

        try {
          println("In DimensionalModel Main Spark Application!")

          spark.conf.set("spark.cosmos.accountKey", mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.analyticalcosmos.keyname")))

          if (spark.conf.get("spark.ec.deleteModelFolder", "false").toBoolean) {
            performMaintenance(config.AccountId, config.AdlsTargetDirectory)
          }

          println(
            s"""Received parameters:
               |Target ADLS Path - ${config.AdlsTargetDirectory}
               |Re-processing Threshold (in minutes) - ${config.ReProcessingThresholdInMins}
               |Account ID - ${config.AccountId},
               |Unique JobRunGuid (CorrelationId) - ${config.JobRunGuid}""".stripMargin)

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid
            , jobName = "DimensionalModel", jobStatus = JobStatus.Started.toString, tenantId = tenantId)

          // Processing DimDate Delta Table
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension.DimMain.main(config.AdlsTargetDirectory, config.ReProcessingThresholdInMins, config.AccountId, config.JobRunGuid, spark)
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact.FactMain.main(config.AdlsTargetDirectory, config.ReProcessingThresholdInMins, config.AccountId, config.JobRunGuid, spark)

          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid
            , jobName = "DimensionalModel", jobStatus = JobStatus.Completed.toString, tenantId = tenantId)
        }
        catch {
          case e =>
            logger.error(s"Error in DomainModel Main Spark Application: ${e.getMessage}", e)
            LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid
              , jobName = "DimensionalModel", JobStatus.Failed.toString, tenantId = tenantId, e.toString)
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