package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common

import org.apache.log4j.Logger
import org.apache.spark.sql.{DataFrame, SaveMode, SparkSession}
import io.delta.tables.DeltaTable

import scala.concurrent.{Future, Promise}
import java.time.format.DateTimeFormatter
import java.time.{ZoneOffset, ZonedDateTime}

import javax.crypto.Mac
import javax.crypto.spec.SecretKeySpec
import org.apache.spark.sql.expressions.Window
import org.apache.spark.sql.functions.{col, lit, row_number, when}

import java.net.{HttpURLConnection, URL}
import scala.concurrent.ExecutionContext.Implicits.global

class Reader(spark: SparkSession, logger: Logger) {
  private def deltaTableExists(deltaTablePath: String): Boolean = {
    try {
      DeltaTable.isDeltaTable(deltaTablePath)
    } catch {
      case _: Throwable => false
    }
  }

  def readAdlsDelta(DeltaPath: String, Entity: String): Option[DataFrame] = {
    val directoryPath = DeltaPath.concat("/DomainModel/").concat(Entity)
    val pathExists = try {
      mssparkutils.fs.ls(directoryPath).nonEmpty
    } catch {
      case _: Exception => false
    }
    if (pathExists){
      val DomainDeltaTable = DeltaTable.forPath(spark, directoryPath)
      val dfDomainDeltaTable = DomainDeltaTable.toDF
      if (deltaTableExists(directoryPath) && !dfDomainDeltaTable.isEmpty) {
        val df = dfDomainDeltaTable
        Some(df)
      } else {
        println(s"Delta table does not exist at path or IsEmpty: $directoryPath")
        None
      }
    } else {
      println(s"Path does not exist or folder is empty: $directoryPath")
      None
    }

  }
  def writeCosmosData(df:DataFrame,Entity:String): Unit = {
    try {
      df.write.format("cosmos.oltp")
        .option("spark.cosmos.accountEndpoint", spark.conf.get("spark.cosmos.accountEndpoint"))
        .option("spark.cosmos.database", spark.conf.get("spark.cosmos.database"))
        .option("spark.cosmos.container", "dehsentinel")
        .option("spark.cosmos.accountKey", spark.conf.get("spark.cosmos.accountKey"))
        .mode(SaveMode.Append)
        .save()
    }
    catch {
      case e: Exception =>
        println(s"Error Checkpointing 'dehsentinel' Asset $Entity: ${e.getMessage}")
        logger.error(s"Error Checkpointing 'dehsentinel' Asset $Entity: ${e.getMessage}")
        throw e
    }
  }

  def logLivyApplicationDetails(df:DataFrame): DataFrame = {
    val conf = spark.sparkContext.getConf
    val applicationId = spark.sparkContext.applicationId
    val applicationName = conf.get("spark.app.name")
    val master = conf.get("spark.master")
    val deployMode = conf.getOption("spark.submit.deployMode").getOrElse("N/A")
    val executorMemory = conf.getOption("spark.executor.memory").getOrElse("N/A")
    val executorCores = conf.getOption("spark.executor.cores").getOrElse("N/A")
    val numExecutors = conf.getOption("spark.executor.instances").getOrElse("N/A")

    val executorMemoryOverhead = conf.getOption("spark.executor.memoryOverhead").getOrElse("N/A")
    val driverMemory = conf.getOption("spark.driver.memory").getOrElse("N/A")
    val driverCores = conf.getOption("spark.driver.cores").getOrElse("N/A")
    val dynamicAllocationEnabled = conf.getOption("spark.dynamicAllocation.enabled").getOrElse("N/A")
    val maxExecutorInstances = conf.getOption("spark.dynamicAllocation.maxExecutors").getOrElse("N/A")

    val details = Seq((applicationId, applicationName, master, deployMode, executorMemory, executorCores
      , numExecutors,executorMemoryOverhead
      ,driverMemory,driverCores,dynamicAllocationEnabled, maxExecutorInstances))

    var dfSparkApplicationDetails = spark.createDataFrame(details).toDF("ApplicationId", "ApplicationName", "Master"
      , "DeployMode", "ExecutorMemory", "ExecutorCores", "NumExecutors"
      ,"ExecutorMemoryOverhead","DriverMemory","DriverCores"
      ,"DynamicAllocationEnabled", "MaxExecutorInstances")

    dfSparkApplicationDetails = dfSparkApplicationDetails.withColumn("NodeSize",
      when(col("ExecutorCores") === "4", "Small")
        .when(col("ExecutorCores") === "8", "Medium")
        .when(col("ExecutorCores") === "16", "Large")
        .when(col("ExecutorCores") === "32", "XLarge")
        .when(col("ExecutorCores") === "64", "XXLarge")
        .otherwise("N/A")
    )

    val dfInput = df.withColumn("row_num", row_number().over(Window.orderBy(lit(1))))
    val dfSparkApplicationDetailsUpdate = dfSparkApplicationDetails.withColumn("row_num", row_number().over(Window.orderBy(lit(1))))

    val combinedDF = dfInput.join(dfSparkApplicationDetailsUpdate, "row_num").drop("row_num")
    combinedDF
  }

  def writeToLogAnalyticsAsync(df: DataFrame,Entity:String, logType: String): Future[Unit] = {
    val promise = Promise[Unit]()
    Future {
      try {
        val logAnalyticsWorkSpaceId = mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.loganalytics.workspaceid"))
        val logAnalyticsSecret = mssparkutils.credentials.getSecret(spark.conf.get("spark.keyvault.name"), spark.conf.get("spark.loganalytics.workspacekeyname"))
        //Append Spark Details
        val combinedDF = logLivyApplicationDetails(df)
        // Convert DataFrame to JSON
        val jsonString = combinedDF.toJSON.collect().mkString("[", ",", "]")
        val body = jsonString.getBytes("UTF-8")
        // Log Analytics Endpoint URL
        val logAnalyticsUrl = s"https://$logAnalyticsWorkSpaceId.ods.opinsights.azure.com/api/logs?api-version=2016-04-01"

        // Get current date and time in the correct format
        val date = ZonedDateTime.now(ZoneOffset.UTC)
        val dateFormatter = DateTimeFormatter.ofPattern("EEE, dd MMM yyyy HH:mm:ss 'GMT'")
        val formattedDate = date.format(dateFormatter)
        val method = "POST"
        val contentLength = body.length
        val contentType = "application/json"
        val resource = "/api/logs"
        val stringToHash = s"$method\n$contentLength\n$contentType\nx-ms-date:$formattedDate\n$resource"

        // Create the signature
        val decodedKey = java.util.Base64.getDecoder.decode(logAnalyticsSecret)
        val sha256_HMAC = Mac.getInstance("HmacSHA256")
        val secretKey = new SecretKeySpec(decodedKey, "HmacSHA256")
        sha256_HMAC.init(secretKey)
        val hashedBytes = sha256_HMAC.doFinal(stringToHash.getBytes("UTF-8"))
        val signature = java.util.Base64.getEncoder.encodeToString(hashedBytes)

        val authorization = s"SharedKey $logAnalyticsWorkSpaceId:$signature"

        val headers = Map(
          "Content-Type" -> contentType,
          "Log-Type" -> logType,
          "x-ms-date" -> formattedDate,
          "Authorization" -> authorization
        )

        val url = new URL(logAnalyticsUrl)
        val connection = url.openConnection.asInstanceOf[HttpURLConnection]
        connection.setRequestMethod("POST")
        headers.foreach { case (key, value) => connection.setRequestProperty(key, value) }
        connection.setDoOutput(true)
        val outputStream = connection.getOutputStream
        outputStream.write(body)
        outputStream.close()

        // Handle response
        if (connection.getResponseCode != 200) {
          println(s"Failed Checkpointing for Entity $Entity to Log Analytics: Response status code: ${connection.getResponseCode} Response text: ${connection.getResponseMessage}")
          throw new RuntimeException(s"Failed Checkpointing for Entity $Entity to Log Analytics: Response status code: ${connection.getResponseCode} Response text: ${connection.getResponseMessage}")
        }

        println(s"Checkpointing successfully posted for Entity $Entity to Log Analytics: Response status code: ${connection.getResponseCode} Response text: ${connection.getResponseMessage}")
        promise.success(())
      } catch {
        case e: Exception =>
          promise.failure(e)
      }
    }
    promise.future
  }
}
