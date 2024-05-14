package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.main
import com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common.CommandLineParser

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
        try {
          println("In DimensionalModel Main Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the DimensionalModel Main Spark Application!")

          val spark = SparkSession.builder
            .appName("DimensionalModelMainSparkApplication")
            .getOrCreate()

          println(
            s"""Received parameters:
               |Target ADLS Path - ${config.AdlsTargetDirectory}
               |Re-processing Threshold (in minutes) - ${config.ReProcessingThresholdInMins}
               |Account ID - ${config.AccountId},
               |Unique JobRunGuid (CorrelationId) - ${config.JobRunGuid}""".stripMargin)

          // Processing DimDate Delta Table
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.dimension.DimMain.main(config.AdlsTargetDirectory,config.ReProcessingThresholdInMins,config.AccountId,config.JobRunGuid,spark)
          com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.fact.FactMain.main(config.AdlsTargetDirectory,config.ReProcessingThresholdInMins,config.AccountId,config.JobRunGuid,spark)

          spark.stop()
        } catch {
          case e: Exception =>
            println(s"Error In DimensionalModel Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In DimensionalModel Main Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In DimensionalModel Main Spark Application!: ${e.getMessage}")
        }
      case None =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
    }
  }
}