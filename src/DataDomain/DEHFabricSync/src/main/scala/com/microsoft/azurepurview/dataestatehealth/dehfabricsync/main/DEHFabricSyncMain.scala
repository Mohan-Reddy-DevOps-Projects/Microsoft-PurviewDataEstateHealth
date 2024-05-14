package com.microsoft.azurepurview.dataestatehealth.dehfabricsync.main
import com.microsoft.azurepurview.dataestatehealth.dehfabricsync.common.{CommandLineParser, LakehouseCopy}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession

import java.util.ResourceBundle

object DEHFabricSyncMain {
  val logger : Logger = Logger.getLogger(getClass.getName)
  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("dehfabricsync")

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
          println("In DEHFabricSync Main Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the DEHFabricSync Main Spark Application!")

          val spark = SparkSession.builder
            .appName("DEHFabricSyncMainSparkApplication")
            .getOrCreate()

          println(
            s"""Received parameters:
               |DEH Tenant Storage - ${config.DEHStorageAccount},
               |Target OneLake Fabric Root Path - ${config.FabricSyncRootPath},
               |Account ID - ${config.AccountId},
               |Account ID - ${config.ProcessDomainModel}
               |Account ID - ${config.ProcessDimensionaModel}""".stripMargin)
          if (config.ProcessDomainModel){
            val lakehouseDomainModelSync = new LakehouseCopy(spark,logger)
            lakehouseDomainModelSync.processLakehouseCopy(config.DEHStorageAccount.concat("/DomainModel"),config.FabricSyncRootPath)
          }
          if (config.ProcessDimensionaModel){
            val lakehouseDimensionalModelSync = new LakehouseCopy(spark,logger)
            lakehouseDimensionalModelSync.processLakehouseCopy(config.DEHStorageAccount.concat("/DimensionalModel"),config.FabricSyncRootPath)
          }
          spark.stop()
        }
        catch
        {
          case e: Exception =>
            println(s"Error In DEHFabricSync Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In DEHFabricSync Main Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In DEHFabricSync Main Spark Application!: ${e.getMessage}")
        }
      case _ =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
    }
  }
}