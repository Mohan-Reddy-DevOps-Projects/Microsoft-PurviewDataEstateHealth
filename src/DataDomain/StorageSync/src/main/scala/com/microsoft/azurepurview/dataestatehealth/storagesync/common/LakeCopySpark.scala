package com.microsoft.azurepurview.dataestatehealth.storagesync.common

import org.apache.spark.sql.{DataFrame, SparkSession}
import org.apache.spark.sql.functions._

// Companion object to encapsulate default parameters
object LakeCopySpark {
  val DefaultFormat = "delta"
  val DefaultSaveMode = "overwrite"
}

/**
 * Class to handle data transfer between storage accounts.
 *
 * @param spark The SparkSession instance used for data operations.
 */
class LakeCopySpark(private val spark: SparkSession) {

  /**
   * Processes folders by transferring data from source to destination.
   *
   * @param source The path of the source storage account.
   * @param destination The path of the destination storage account.
   * @param format The format of the data (e.g., "csv", "parquet", "delta").
   * @param saveMode The save mode for writing the data (e.g., "overwrite").
   */
  def processFolders(source: String, destination: String
                     , format: String = LakeCopySpark.DefaultFormat
                     , saveMode: String = LakeCopySpark.DefaultSaveMode): Unit = {
    val rootDirectories = getRootDirectories(source)
    rootDirectories.foreach { folderName =>
      val sourcePath = s"$source/$folderName"
      val destinationPath = s"$destination/$folderName"
      transferData(sourcePath, destinationPath, format, saveMode)
    }
  }

  /**
   * Retrieves the names of root directories from the specified storage account path.
   *
   * @param storageAccountPath The root path of the storage account.
   * @return A sequence of directory names.
   */
  private def getRootDirectories(storageAccountPath: String): Seq[String] = {
    try {
      mssparkutils.fs.ls(storageAccountPath).map(_.name).toSeq
    } catch {
      case e: Exception =>
        throw new RuntimeException(s"Failed to list directories at path: $storageAccountPath", e)
    }
  }

  /**
   * Transfers data from a source path to a destination path using a specified format and save mode.
   *
   * @param sourcePath The path of the source directory.
   * @param destinationPath The path of the destination directory.
   * @param format The format of the data (e.g., "csv", "parquet", "delta").
   * @param saveMode The save mode for writing the data (e.g., "overwrite").
   */
  private def transferData(sourcePath: String, destinationPath: String, format: String, saveMode: String): Unit = {
    try {
      // Read data from the source directory into a DataFrame
      val df: DataFrame = spark.read.format(format).load(sourcePath)

      println("Row Count of Source Started:")
      print(df.count())
      println("Row Count of Source Completed:")

      // Write the DataFrame to the destination directory with the specified save mode
      df.write
        .format(format)
        .mode(saveMode)
        .save(destinationPath)

      println(s"Data successfully transferred from $sourcePath to $destinationPath.")

    } catch {
      case e: Exception =>
        println(s"Error transferring data from $sourcePath to $destinationPath: ${e.getMessage}")
        e.printStackTrace() // Log stack trace for debugging purposes
    }
  }
}
