package com.microsoft.azurepurview.dataestatehealth.domainmodel.TestDomainModelMain
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.CommandLineParser
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession
import java.util.ResourceBundle

object DomainModelMain {
  val logger : Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String]): Unit = {

    val resourceBundle = ResourceBundle.getBundle("domainmodeltest")
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
          println("In TestDimensionalModel Main Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the TestDimensionalModel Main Spark Application!")

          val spark = SparkSession.builder
            .appName("TestDimensionalModelMainSparkApplication")
            .getOrCreate()

          println(
            s"""Received parameters:
               |Target ADLS Path - ${config.AdlsTargetDirectory}""".stripMargin)

          // Test BusinessDomain Delta Table
          com.microsoft.azurepurview.dataestatehealth.domainmodel.TestBusinessDomain.BusinessDomainMain.main(Array(config.AdlsTargetDirectory),spark)
          spark.stop()
        }
        catch
        {
          case e: Exception =>
            println(s"Error In TestDimensionalModel Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In TestDimensionalModel Main Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In TestDimensionalModel Main Spark Application!: ${e.getMessage}")
        }
      case _ =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
    }
  }
}
