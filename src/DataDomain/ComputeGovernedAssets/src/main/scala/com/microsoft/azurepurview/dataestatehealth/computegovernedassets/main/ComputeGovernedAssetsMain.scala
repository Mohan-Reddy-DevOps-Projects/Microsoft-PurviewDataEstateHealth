package com.microsoft.azurepurview.dataestatehealth.computegovernedassets.main

import com.microsoft.azurepurview.dataestatehealth.computegovernedassets.common.{CommandLineParser, LogAnalyticsLogger}
import org.apache.log4j.{Level, Logger}
import org.apache.spark.sql.{AnalysisException, SparkSession, functions => F}
import com.google.gson.Gson
import java.util.ResourceBundle

object ComputeGovernedAssetsMain {
  val logger: Logger = Logger.getLogger(getClass.getName)

  def main(args: Array[String]): Unit = {
    val resourceBundle = ResourceBundle.getBundle("computegovernedassets")

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
        logger.info("Started the ComputeGovernedAssets Main Spark Application!")

        val spark = SparkSession.builder
          .appName("ComputeGovernedAssetsMainSparkApplication")
          .getOrCreate()

        var totalAssetCountInDataMap = 0L;
        var assetWithTermCountInDataMap = 0L;

        try {
          println("In ComputeGovernedAssets Main Spark Application!")

          spark.conf.set("spark.cosmos.accountKey", mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.analyticalcosmos.keyname")))

          println(
            s"""Received parameters:
               |Account ID - ${config.AccountId},
               |Unique JobRunGuid (CorrelationId) - ${config.JobRunGuid}""".stripMargin)

          // Initialize LogAnalyticsConfig with Spark session
          LogAnalyticsLogger.initialize(spark)
          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            jobStatus = "Started")

          // Print all configurations
          /*println("Spark Configuration:")
          spark.conf.getAll.foreach { case (key, value) =>
            println(s"$key = $value")
          }*/

          // Read OT data
          var rddContainerName = spark.conf.get("spark.rdd.containerName")
          var rddAccountName = spark.conf.get("spark.rdd.accountName")
          var rddDnsZone = spark.conf.get("spark.rdd.dnsZone")
          var rddSasToken = spark.conf.get("spark.rdd.sasToken")
          println(s"rddContainerName: $rddContainerName")
          spark.sparkContext.hadoopConfiguration.set(
            s"fs.azure.sas.$rddContainerName.$rddAccountName.blob.core.windows.net",
            rddSasToken
          )
          val allAssetsInDataMap = spark.read
            .format("delta")
            .load(s"abfss://$rddContainerName@$rddAccountName.$rddDnsZone.dfs.storage.azure.net/AtlasRdd/AtlasDeltaDataset" )
          totalAssetCountInDataMap = allAssetsInDataMap.count();

          var assetsWithTermInDataMap = allAssetsInDataMap
            .filter(
              F.col("mainAsset.relationshipAttributes.meanings").isNotNull// Check if 'meanings' array in 'mainAsset' is not empty
              || F.expr("AGGREGATE(schemaEntities, 0L, (total, col) -> total + IF(col.relationshipAttributes.meanings IS NOT NULL, 1L, 0L)) > 0") // Check if column has term
            )
            .select(F.col("mainAsset.guid").alias("assetId"))
          assetsWithTermInDataMap.show();

          assetWithTermCountInDataMap = assetsWithTermInDataMap.count();
        }
        catch {
          case e: AnalysisException =>
            // Check for known exception. No RDD path means there is no data asset, not throw exception
            if (e.getMessage.contains("Path does not exist")) {
              logger.info("Caught AnalysisException: Path does not exist", e)
            }
            else {
              throw e
            }
          case e =>
            logger.error(s"Error in ComputeGovernedAssets Main Spark Application: ${e.getMessage}", e)
            throw e // Re-throw the exception to ensure the job failure is reported correctly
        } finally {
          println(s"Total asset count in Data Map: $totalAssetCountInDataMap")
          println(s"Asset with term count in Data Map: $assetWithTermCountInDataMap")

          logger.info(s"Total assets count in Data Map: $totalAssetCountInDataMap")
          logger.info(s"Assets with term count in Data Map: $assetWithTermCountInDataMap")

          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            if (Thread.currentThread.isInterrupted) "Cancelled" else "Completed",
            new Gson().toJson(ComputeGovernedAssetsCountResult(
              TotalAssetCountInDataMap = totalAssetCountInDataMap,
              AssetWithTermCountInDataMap = assetWithTermCountInDataMap)))
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