package com.microsoft.azurepurview.dataestatehealth.controls.util

import org.apache.log4j.Logger
import org.apache.spark.sql.DataFrame
import com.microsoft.azurepurview.dataestatehealth.controls.common.CommonUtils
import com.microsoft.azurepurview.dataestatehealth.controls.constants.Constants

/**
 * Shared utilities for the Control Jobs module.
 * Provides helper methods for data processing, validation and safe execution.
 */
object ControlJobUtils {
  
  import Constants.Storage._
  
  /**
   * Extracts account name from datasource FQN
   *
   * @param datasourceFQN The fully qualified datasource name
   * @return The extracted account name
   */
  def extractAccountName(datasourceFQN: String): String = {
    if (datasourceFQN == null || datasourceFQN.isEmpty) {
      return ""
    }
    
    val pattern = DATASOURCE_HOST_PATTERN.r
    pattern.findFirstMatchIn(datasourceFQN).map(_.group(1)).getOrElse("")
  }
  
  /**
   * Constructs a storage endpoint string from components
   *
   * @param fileSystem The file system name
   * @param datasourceFQN The datasource fully qualified name
   * @param folderPath The folder path
   * @return A fully constructed storage endpoint string
   */
  def constructStorageEndpoint(fileSystem: String, datasourceFQN: String, folderPath: String): String = {
    if (fileSystem == null || datasourceFQN == null) {
      return ""
    }
    
    val accountName = extractAccountName(datasourceFQN)
    if (accountName.isEmpty) {
      return ""
    }
    
    val sanitizedFolderPath = if (folderPath == null) "" else folderPath
    s"$ABFSS_PREFIX$fileSystem@$accountName/$sanitizedFolderPath"
  }
  
  /**
   * Checks if a DataFrame is empty in a safe manner
   *
   * @param df The DataFrame to check
   * @return true if the DataFrame is empty or null, false otherwise
   */
  def isDataFrameEmpty(df: DataFrame): Boolean = {
    if (df == null) {
      return true
    }
    
    try {
      df.isEmpty
    } catch {
      case e: Exception => 
        throw new RuntimeException(s"Failed to check if DataFrame is empty: ${e.getMessage}", e)
    }
  }
  
  /**
   * Gets the row count of a DataFrame in a safe manner
   *
   * @param df The DataFrame to count rows for
   * @param logger The logger to use
   * @return The row count or 0 if an error occurs
   */
  def getDataFrameCount(df: DataFrame, logger: Logger): Long = {
    try {
      df.count()
    } catch {
      case e: Exception =>
        logger.error(s"Error getting DataFrame count: ${e.getMessage}", e)
        throw new RuntimeException(s"Failed to get DataFrame count: ${e.getMessage}", e)
    }
  }
  
  /**
   * Gets the schema of a DataFrame in a safe manner
   *
   * @param df The DataFrame to get the schema for
   * @param logger The logger to use
   * @return The schema as a string or an empty string if an error occurs
   */
  def getDataFrameSchema(df: DataFrame, logger: Logger): String = {
    try {
      df.schema.treeString
    } catch {
      case e: Exception =>
        logger.error(s"Error getting DataFrame schema: ${e.getMessage}", e)
        throw new RuntimeException(s"Failed to get DataFrame schema: ${e.getMessage}", e)
    }
  }
} 