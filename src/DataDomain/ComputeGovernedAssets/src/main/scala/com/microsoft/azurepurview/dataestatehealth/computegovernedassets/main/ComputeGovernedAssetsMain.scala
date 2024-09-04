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

        var countOfAssetsInDataMap = 0L;
        var countOfAssetInDG = 0L;
        var countOfGovernedAsset = 0L;
        var exceptionMsg = "";

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
          println("Spark Configuration:")
          /*spark.conf.getAll.foreach { case (key, value) =>
            println(s"$key = $value")
          }*/

          // Read DataMap OT data
          val rddAssetsFormat = spark.conf.get("spark.rdd.assetsFormat")
          val rddAssetsPath = spark.conf.get("spark.rdd.assetsPath")
          println(s"rddAssetsPath: $rddAssetsPath")
          val allAssetsInDataMap = spark.read
            .format(rddAssetsFormat)
            .load(rddAssetsPath)
            .select(F.col("mainAsset.attributes.qualifiedName").alias("assetFQN"))
          countOfAssetsInDataMap = allAssetsInDataMap.count();

          // Read DG data
          val domainModelAssetsFormat = spark.conf.get("spark.domainModel.assetsFormat")
          val domainModelAssetsPath = spark.conf.get("spark.domainModel.assetsPath")
          println(s"domainModelAssetsPath: $domainModelAssetsPath")
          val allAssetsInDG = spark.read
            .format(domainModelAssetsFormat)
            .load(domainModelAssetsPath)
            .filter(F.col("OperationType").notEqual("Delete"))
          countOfAssetInDG = allAssetsInDG.count()

          // Check DG assets exist in DataMap
          val assetInDataMapFQNSet: Set[String] = allAssetsInDataMap
            .select("assetFQN")
            .rdd
            .map(row => row.getAs[String]("assetFQN"))
            .collect()
            .toSet

          val allGovernedAssets = allAssetsInDG
            .filter(row =>
              assetInDataMapFQNSet.contains(row.getAs[String]("FullyQualifiedName"))
            )
          countOfGovernedAsset = allGovernedAssets.count()
        }
        catch {
          case e: AnalysisException =>
            e.printStackTrace()
            exceptionMsg = e.getMessage
            // Check for known exception. No RDD path means there is no data asset, not throw exception
            if (e.getMessage.contains("Path does not exist")) {
              logger.info("Caught AnalysisException: RDD path does not exist", e)
            }
            else {
              logger.error(s"Error in ComputeGovernedAssets Main Spark Application: ${e.getMessage}", e)
              throw e
            }
          case e =>
            e.printStackTrace()
            logger.error(s"Error in ComputeGovernedAssets Main Spark Application: ${e.getMessage}", e)
            exceptionMsg = e.getMessage
            throw e // Re-throw the exception to ensure the job failure is reported correctly
        } finally {
          println(s"CountOfAssetsInDataMap:$countOfAssetsInDataMap")
          println(s"CountOfAssetsInDG:$countOfAssetInDG")
          println(s"CountOfGovernedAssets:$countOfGovernedAsset")

          logger.info(s"CountOfAssetInDataMap: $countOfAssetsInDataMap")
          logger.info(s"CountOfAssetInDG: $countOfAssetInDG")
          logger.info(s"CountOfGovernedAsset: $countOfGovernedAsset")

          LogAnalyticsLogger.checkpointJobStatus(accountId = config.AccountId, jobRunGuid = config.JobRunGuid,
            if (Thread.currentThread.isInterrupted) "Cancelled" else "Completed",
            new Gson().toJson(ComputeGovernedAssetsCountResult(
              CountOfAssetsInDataMap = countOfAssetsInDataMap,
              CountOfAssetsInDG = countOfAssetInDG,
              CountOfGovernedAssets = countOfGovernedAsset,
              ExceptionMsg = exceptionMsg)))
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