package com.microsoft.azurepurview.dataestatehealth.dimensionalmodel.common


import java.io.{BufferedReader, InputStreamReader, OutputStream}
import java.net.{HttpURLConnection, URL}
import java.nio.charset.StandardCharsets
import java.time.{ZonedDateTime, ZoneOffset}
import java.time.format.DateTimeFormatter
import java.util.Base64
import javax.crypto.Mac
import javax.crypto.spec.SecretKeySpec
import scala.util.control.NonFatal
import java.time.Instant
import java.util.UUID
import java.sql.Timestamp
import com.google.gson.Gson
import org.apache.spark.sql.SparkSession

object LogAnalyticsLogger {
  // Configuration variables
  private var logAnalyticsWorkSpaceId: String = "defaultWorkspaceId"
  private var logAnalyticsSecret: String = "defaultSecret"

  def initialize(spark: SparkSession): Unit = {
    try {
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

  def checkpointJobStatus(accountId: String, jobRunGuid: String, jobStatus: String): Unit = {
    try {
      val data = JobStatusSchema(
        Id = UUID.randomUUID().toString,
        AccountId = accountId,
        JobId = jobRunGuid,
        JobName = "DimensionalModel",
        JobStatus = jobStatus,
        JobCompletionTime = Timestamp.from(Instant.now()).toString
      )

      // Convert data to JSON string
      val jsonString = new Gson().toJson(data)

      println(s"Checkpoint Job Status: $jsonString")

      // Write the JSON string to Log Analytics asynchronously
      writeToLogAnalyticsAsync(jsonString, "DEH_Job_Logs")

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
