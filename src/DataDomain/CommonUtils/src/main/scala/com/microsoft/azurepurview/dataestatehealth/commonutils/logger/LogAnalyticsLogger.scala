package com.microsoft.azurepurview.dataestatehealth.commonutils.logger

import com.google.gson.Gson
import org.apache.spark.sql.SparkSession

import java.io.{BufferedReader, InputStreamReader, OutputStream}
import java.net.{HttpURLConnection, URL}
import java.nio.charset.StandardCharsets
import java.sql.Timestamp
import java.time.format.DateTimeFormatter
import java.time.{Instant, ZoneOffset, ZonedDateTime}
import java.util.{Base64, UUID}
import javax.crypto.Mac
import javax.crypto.spec.SecretKeySpec
import scala.util.control.NonFatal

object LogAnalyticsLogger {
  // Configuration variables
  private var logAnalyticsWorkSpaceId: String = "defaultWorkspaceId"
  private var logAnalyticsSecret: String = "defaultSecret"
  private var sparkSession: SparkSession = _

  def initialize(spark: SparkSession): Unit = {
    try {
      this.sparkSession = spark
      val keyVaultName = spark.conf.get("spark.keyvault.name")
      val workspaceIdSecretName = spark.conf.get("spark.loganalytics.workspaceid")
      val workspaceKeySecretName = spark.conf.get("spark.loganalytics.workspacekeyname")
      logAnalyticsWorkSpaceId = mssparkutils.credentials.getSecret(keyVaultName, workspaceIdSecretName)
      logAnalyticsSecret = mssparkutils.credentials.getSecret(keyVaultName, workspaceKeySecretName)
    } catch {
      case e: Exception =>
        throw new RuntimeException("Failed to initialize LogAnalyticsService", e)
    }
  }

  def checkpointJobStatus(accountId: String, jobRunGuid: String, jobName: String, jobStatus: String,
                          tenantId: String, errorMessage: String="", correlationId: String=""): Unit = {
    try {
      val data = JobStatusSchema(
        Id = UUID.randomUUID().toString,
        AccountId = accountId,
        JobId = jobRunGuid,
        JobName = jobName,
        JobStatus = jobStatus,
        JobCompletionTime = Timestamp.from(Instant.now()).toString,
        PurviewTenantId = tenantId,
        ErrorMessage = errorMessage,
        CorrelationId = correlationId
      )

      // Convert data to JSON string
      val jsonString = new Gson().toJson(data)

      println(s"Checkpoint Job Status: $jsonString")

      // Determine which table to use based on feature flag
      val logTableName = try {
        val switchToNewControlsFlow = sparkSession.conf.get("spark.ec.switchToNewControlsFlow", "false").toBoolean
        if (switchToNewControlsFlow) "DEH_Job_Logs_V2" else "DEH_Job_Logs"
      } catch {
        case _: Exception => "DEH_Job_Logs" // Default to original table if flag check fails
      }

      // Write the JSON string to Log Analytics asynchronously
      writeToLogAnalyticsAsync(jsonString, logTableName)

      println("CheckpointJobStatus operation completed successfully.")
    } catch {
      case e: Exception =>
        // Log the error with stack trace
        println("Error during checkpointJobStatus operation.", e)
    }
  }

  private def computeSignature(secret: String, method: String, contentLength: Long, contentType: String, date: String, resource: String): String = {
    val stringToSign = s"$method\n$contentLength\n$contentType\nx-ms-date:$date\n$resource"
    val decodedKey = Base64.getDecoder.decode(secret)
    val sha256_HMAC = Mac.getInstance("HmacSHA256")
    val secretKey = new SecretKeySpec(decodedKey, "HmacSHA256")
    sha256_HMAC.init(secretKey)
    val hashedBytes = sha256_HMAC.doFinal(stringToSign.getBytes(StandardCharsets.UTF_8))
    Base64.getEncoder.encodeToString(hashedBytes)
  }

  private def writeToLogAnalyticsAsync(body: String, logType: String): Unit = {
    val logAnalyticsUrl = s"https://${logAnalyticsWorkSpaceId}.ods.opinsights.azure.com/api/logs?api-version=2016-04-01"
    var connection: HttpURLConnection = null
    var outputStream: OutputStream = null
    var errorStream: BufferedReader = null
    try {
      // Prepare request details
      val date = ZonedDateTime.now(ZoneOffset.UTC).format(DateTimeFormatter.ofPattern("EEE, dd MMM yyyy HH:mm:ss 'GMT'"))
      val signature = computeSignature(logAnalyticsSecret, "POST", body.length, "application/json", date, "/api/logs")
      val authorization = s"SharedKey ${logAnalyticsWorkSpaceId}:$signature"

      // Setup HTTP connection
      val url = new URL(logAnalyticsUrl)
      connection = url.openConnection().asInstanceOf[HttpURLConnection]
      connection.setRequestMethod("POST")
      connection.setRequestProperty("Content-Type", "application/json")
      connection.setRequestProperty("Log-Type", logType)
      connection.setRequestProperty("x-ms-date", date)
      connection.setRequestProperty("Authorization", authorization)
      connection.setDoOutput(true)

      // Write data to the output stream
      outputStream = connection.getOutputStream
      try {
        outputStream.write(body.getBytes(StandardCharsets.UTF_8))
        outputStream.flush()
      } finally {
        outputStream.close()
      }

      // Handle the response
      connection.getResponseCode match {
        case 200 =>
          println("Successfully logged")
        case code =>
          // Read the error stream to avoid connection hang
          errorStream = new BufferedReader(new InputStreamReader(connection.getErrorStream, StandardCharsets.UTF_8))
          val errorMessage = errorStream.lines().toArray.mkString("\n")
          throw new RuntimeException(s"Failed with status code $code. Error: $errorMessage")
      }
    } catch {
      case NonFatal(e) =>
        throw e // Re-throw the exception after logging or handling it
    } finally {
      // Ensure streams and connections are closed
      if (outputStream != null) outputStream.close()
      if (errorStream != null) errorStream.close()
      if (connection != null) connection.disconnect()
    }
  }
}
