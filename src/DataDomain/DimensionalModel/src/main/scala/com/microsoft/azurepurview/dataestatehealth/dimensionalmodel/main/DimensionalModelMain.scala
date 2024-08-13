package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.main

import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.{CommandLineParser, LogAnalyticsLogger}

import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession
import java.util.ResourceBundle

object DimensionalModelMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("dimensionalmodel")

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
        logger.setLevel(Level.INFO)
        logger.info("Started the DimensionalModel Main Spark Application!")

        val spark = SparkSession.builder
          .appName("DimensionalModelMainSparkApplication")
          .getOrCreate()
        try {
          println("In DimensionalModel Main Spark Application!")


          spark.conf.set("spark.cosmos.accountKey",mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.analyticalcosmos.keyname")))

          println(
            s"""Received parameters:
               |Target ADLS Path - ${config.AdlsTargetDirectory}
               |Re-processing Threshold (in minutes) - ${config.ReProcessingThresholdInMins}
               |Account ID - ${config.AccountId},
               |Unique JobRunGuid (CorrelationId) - ${config.JobRunGuid}""".stripMargin)

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid
            , jobStatus = "Started")

          // Processing DimDate Delta Table
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension.DimMain.main(config.AdlsTargetDirectory,config.ReProcessingThresholdInMins,config.AccountId,config.JobRunGuid,spark)
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact.FactMain.main(config.AdlsTargetDirectory,config.ReProcessingThresholdInMins,config.AccountId,config.JobRunGuid,spark)

        }
        catch {
          case e =>
            logger.error(s"Error in DomainModel Main Spark Application: ${e.getMessage}", e)
            throw e // Re-throw the exception to ensure the job failure is reported correctly
        } finally {
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            if (Thread.currentThread.isInterrupted) "Cancelled" else "Completed")
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