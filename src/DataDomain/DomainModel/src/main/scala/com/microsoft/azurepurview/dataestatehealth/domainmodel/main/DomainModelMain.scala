package com.microsoft.azurepurview.dataestatehealth.domainmodel.main
import com.microsoft.azurepurview.dataestatehealth.domainmodel.common.CommandLineParser
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.SparkSession
import java.util.ResourceBundle

object DomainModelMain {
  val logger : Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("domainmodel")

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
          println("In DomainModel Main Spark Application!")
          logger.setLevel(Level.INFO)
          logger.info("Started the DomainModel Main Spark Application!")

          val spark = SparkSession.builder
            .appName("DomainModelMainSparkApplication")
            .getOrCreate()

          println(
            s"""Received parameters:
               |Source Cosmos Linked Service - ${config.CosmosDBLinkedServiceName},
               |Target ADLS Path - ${config.AdlsTargetDirectory},
               |Account ID - ${config.AccountId},
               |Refresh Type - ${config.RefreshType},
               |Re-processing Threshold (in minutes) - ${config.ReProcessingThresholdInMins},
               |Unique JobRunGuid (CorrelationId) - ${config.JobRunGuid}""".stripMargin)

        // Processing BusinessDomain Delta Table
        com.microsoft.azurepurview.dataestatehealth.domainmodel.businessdomain.BusinessDomainMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType, config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.dataproduct.DataProductMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.glossaryterm.GlossaryTermMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.relationship.RelationshipMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.dataasset.DataAssetMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.accesspolicyset.AccessPolicySetMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.subscription.SubscriptionMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        com.microsoft.azurepurview.dataestatehealth.domainmodel.dataquality.DataQualityMain.main(Array(config.CosmosDBLinkedServiceName, config.AdlsTargetDirectory, config.AccountId, config.RefreshType,config.JobRunGuid),spark,config.ReProcessingThresholdInMins)
        spark.stop()
      }
        catch
        {
          case e: Exception =>
            println(s"Error In DomainModel Main Spark Application!: ${e.getMessage}")
            logger.error(s"Error In DomainModel Main Spark Application!: ${e.getMessage}")
            throw new IllegalArgumentException(s"Error In DomainModel Main Spark Application!: ${e.getMessage}")
        }
      case _ =>
        println("Failed to parse command line arguments.")
        sys.exit(1)
  }
}}