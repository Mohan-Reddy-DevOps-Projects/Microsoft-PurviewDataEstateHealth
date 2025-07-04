package com.microsoft.azurepurview.dataestatehealth.commonutils.writer

import com.microsoft.azurepurview.dataestatehealth.commonutils.logger.SparkLogging
import com.microsoft.azurepurview.dataestatehealth.commonutils.utils.Utils
import io.delta.tables.DeltaTable
import org.apache.spark.sql.functions._
import org.apache.spark.sql.{DataFrame, Row, SparkSession}

// Add companion object before the DataWriter class definition
object DataWriter {
  // Set of column names that need HTML tag removal
  // Using Set for more efficient membership checking
  val columnsToCleanHtmlTags: Set[String] = Set(
    "BusinessDomainDescription",
    "CriticalDataElementDescription",
    "AssetDescription",
    "ColumnDescription",
    "DataProductDescription",
    "UseCases",
    "GlossaryDescription")
  
  // Define common HTML tags to ensure we're only matching real HTML and not any text in brackets
  private val commonHtmlTags = 
    """a|abbr|address|area|article|aside|audio|b|base|bdi|bdo|blockquote|body|br|button|
       |canvas|caption|cite|code|col|colgroup|data|datalist|dd|del|details|dfn|dialog|div|
       |dl|dt|em|embed|fieldset|figcaption|figure|footer|form|h1|h2|h3|h4|h5|h6|head|header|
       |hr|html|i|iframe|img|input|ins|kbd|label|legend|li|link|main|map|mark|meta|meter|
       |nav|noscript|object|ol|optgroup|option|output|p|param|picture|pre|progress|q|rp|rt|
       |ruby|s|samp|script|section|select|small|source|span|strong|style|sub|summary|sup|
       |table|tbody|td|template|textarea|tfoot|th|thead|
       |time|title|tr|track|u|ul|var|video|wbr""".stripMargin.replaceAll("\\s+", "")
  
  // Regular expression to match real HTML tags using the defined list of tags
  // This will only match proper HTML tags and ignore arbitrary text in angle brackets
  private val htmlTagPattern = "</?(" + commonHtmlTags + ")(\\s+[a-zA-Z][a-zA-Z0-9-]*(\\s*=\\s*(\"[^\"]*\"|'[^']*'|[^\\s'\">=]+))?)*\\s*/?>|<!--.*?-->"
}

class DataWriter(spark: SparkSession) extends SparkLogging {

  /**
   * Removes HTML tags from specified columns in the DataFrame
   *
   * @param df The input DataFrame
   * @return DataFrame with HTML tags removed from specified columns
   */
  private def cleanHtmlTagsFromColumns(df: DataFrame): DataFrame = {
    import org.apache.spark.sql.functions.regexp_replace
    
    // Get the intersection of columns in DataFrame and columns to clean
    val columnsToClean = df.columns.toSet.intersect(DataWriter.columnsToCleanHtmlTags)
    
    // If no columns need cleaning, return the original DataFrame
    if (columnsToClean.isEmpty) {
      logger.info(s"No columns matched for HTML tag cleaning in the current DataFrame")
      return df
    }
    
    logger.info(s"Cleaning HTML tags from columns: ${columnsToClean.mkString(", ")}")
    
    // Apply HTML tag removal to each column that needs cleaning
    columnsToClean.foldLeft(df) { (currentDf, columnName) =>
      logger.debug(s"Removing HTML tags from column: $columnName")
      currentDf.withColumn(columnName, 
        regexp_replace(col(columnName), DataWriter.htmlTagPattern, ""))
    }
  }

  def writeData(df: DataFrame, adlsTargetDirectory: String, reProcessingThresholdInMins: Int, entityName: String,
                keyColumns: Seq[String] = Seq(""), refreshType: String = "full", operationType: String = "Merge"): Unit = {
    try {
      val targetPath = s"$adlsTargetDirectory/$entityName"

      logger.info(s"Starting write operation for entity: $entityName to path: $targetPath with refreshType: $refreshType")

      // Clean HTML tags from specified columns
      val cleanedDf = cleanHtmlTagsFromColumns(df)

      // Check if the Delta table has been refreshed within the specified time threshold
      if (isDeltaTableRefreshedWithinXMinutes(adlsTargetDirectory, entityName, reProcessingThresholdInMins)) {
        logger.info(s"Delta table for entity: $entityName was refreshed within the last $reProcessingThresholdInMins minutes. No action taken.")
        return // Exit early if no refresh is needed
      }

      val coldStartDF = Utils.createColdStartDataFrame(spark, cleanedDf.schema)

      // Handle the different types of refresh operations
      refreshType match {
        case "full" =>
          logger.info("Performing full write operation.")
          writeFull(if (cleanedDf.isEmpty) coldStartDF else cleanedDf, targetPath)
        case "incremental" =>
          logger.info("Handling incremental refresh.")
          handleIncrementalRefresh(cleanedDf, targetPath, entityName, coldStartDF, keyColumns, operationType)
        case _ =>
          logger.error(s"Unsupported refreshType: $refreshType")
          throw new IllegalArgumentException(s"Unsupported refreshType: $refreshType") // Handle unsupported refresh types
      }

      logger.info(s"Write operation completed for entity: $entityName.")
    } catch {
      case e: IllegalArgumentException =>
        logger.error(s"Invalid argument: ${e.getMessage}", e)
        throw e // Re-throw to signal error
      case e: Exception =>
        logger.error(s"An unexpected error occurred during write operation for entity: $entityName", e)
        throw e // Re-throw to signal error
    }
  }

  /**
   * Handles the logic for incremental refresh.
   *
   * @param df          The DataFrame to write.
   * @param targetPath  The path of the target Delta table.
   * @param entityName  The name of the entity being processed.
   * @param coldStartDF A DataFrame to use for cold starts when needed.
   */
  private def handleIncrementalRefresh(df: DataFrame, targetPath: String, entityName: String,
                                       coldStartDF: DataFrame, keyColumns: Seq[String],
                                       operationType: String): Unit = {
    try {
      // Check if the Delta table exists
      if (DeltaTable.isDeltaTable(targetPath)) {
        logger.info(s"Delta Table $entityName exists. Handling incremental merge.")
        handleIncrementalMerge(df, targetPath, entityName, keyColumns, coldStartDF, operationType)
      } else {
        // If it does not exist, perform a full overwrite
        logger.warn(s"Delta Table $entityName does not exist for incremental merge. Performing Full Overwrite...")
        writeFull(if (df.isEmpty) coldStartDF else df, targetPath) // Use coldStartDF if the incoming df is empty
      }
    } catch {
      case e: Exception =>
        logger.error(s"Error in handling incremental refresh for entity: $entityName", e)
        throw e
    }
  }

  /**
   * Handles the logic for merging data into the Delta table incrementally.
   *
   * @param df                 The DataFrame to merge.
   * @param dfTargetDeltaTable The DeltaTable representing the target data.
   * @param entityName         The name of the entity being processed.
   */
  private def handleIncrementalMerge(df: DataFrame, targetPath: String, entityName: String
                                     , keyColumns: Seq[String], coldStartDF: DataFrame
                                     , operationType: String): Unit = {
    try {
      val dfTargetDeltaTable = DeltaTable.forPath(spark, targetPath) // Load the target Delta table
      val dfTarget = dfTargetDeltaTable.toDF // Convert DeltaTable to DataFrame

      // Check the states of the source and target DataFrames
      (df.isEmpty, dfTarget.isEmpty) match {
        case (false, false) =>
          logger.info(s"Both source and target DataFrames are non-empty. Performing merge to $entityName.")
          performMerge(df, dfTargetDeltaTable, keyColumns, operationType) // Both DataFrames are non-empty, perform merge
        case (false, true) =>
          logger.warn(s"Target Delta Table $entityName is empty. Performing Full Overwrite...")
          writeFull(df, targetPath) // Target is empty, perform a full overwrite
        case (true, true) =>
          logger.info(s"Both source and target DataFrames are empty. Create cold start record for $entityName")
          writeFull(coldStartDF, targetPath)
        case (true, false) =>
          logger.info("Source DataFrame is empty. No action taken.")
        // Source DataFrame is empty, do nothing
      }
    } catch {
      case e: Exception =>
        logger.error(s"Error in handling incremental merge for entity: $entityName", e)
        throw e
    }
  }

  private def performMerge(df: DataFrame, dfTargetDeltaTable: DeltaTable, keyColumns: Seq[String],
                           operationType: String): Unit = {

    val eventProcessingTimeColumnExists = dfTargetDeltaTable.toDF.columns.contains("EventProcessingTime")
    val operationTypeColumnExists = df.columns.contains("OperationType")

    if (eventProcessingTimeColumnExists && operationTypeColumnExists) {
      performMergeWithFilters(df, dfTargetDeltaTable, keyColumns, operationType)
    } else {
      performMergeWithoutFilters(df, dfTargetDeltaTable, keyColumns, operationType)
    }
  }

  private def performMergeWithFilters(df: DataFrame, dfTargetDeltaTable: DeltaTable, keyColumns: Seq[String],
                                      operationType:String): Unit = {

    logger.info("Calculating maximum event processing time from the target DataFrame.")

    // Create the merge condition by combining key columns
    val mergeCondition = keyColumns.map(key => s"target.$key = source.$key").mkString(" AND ")

    // Get the maximum EventProcessingTime
    val maxEventProcessingTime = dfTargetDeltaTable.toDF
      .agg(max("EventProcessingTime")).as("maxEventProcessingTime")
      .first()
      .getLong(0)

    // Prepare DataFrames for merging (updates/inserts) and deletions
    val filteredMergeDfSource = df.filter(lower(col("OperationType")) =!= "delete")
      .filter(col("EventProcessingTime") > maxEventProcessingTime)

    val filteredDeleteDfSource = df.filter(lower(col("OperationType")) === "delete")

    // Perform the merge for updates and inserts
    logger.info("Performing merge for updates and inserts.")
    val mergeBuilder = dfTargetDeltaTable.as("target").merge(filteredMergeDfSource.as("source"), mergeCondition)

    logger.info(s"Max Event Processing Time Prior to Update:=======> ${maxEventProcessingTime.toString}")
    // Apply the appropriate merge operation
    operationType match {
      case "InsertOnly" => mergeBuilder.whenNotMatched().insertAll().execute()
      case _ => mergeBuilder.whenMatched().updateAll().whenNotMatched().insertAll().execute()
    }

    logger.info(s"Max Event Processing Time Prior to Delete:=======> ${maxEventProcessingTime.toString}")
    if (operationType != "InsertOnly"){
      // Perform the delete operation
      logger.info("Performing merge for deletions.")
      dfTargetDeltaTable.as("target")
        .merge(filteredDeleteDfSource.as("source"), mergeCondition)
        .whenMatched.delete()
        .execute()
    }

  }

  private def performMergeWithoutFilters(df: DataFrame, dfTargetDeltaTable: DeltaTable, keyColumns: Seq[String],
                                         operationType:String): Unit = {

    // Perform the merge for updates and inserts
    logger.info("Performing merge for updates and inserts.")

    // Create the merge condition by combining key columns
    val mergeCondition = keyColumns.map(key => s"target.$key = source.$key").mkString(" AND ")

    val mergeBuilder = dfTargetDeltaTable.as("target").merge(df.as("source"), mergeCondition)

    // Apply the appropriate merge operation
    operationType match {
      case "InsertOnly" => mergeBuilder.whenNotMatched().insertAll().execute()
      case _ => mergeBuilder.whenMatched().updateAll().whenNotMatched().insertAll().execute()
    }
  }

  /**
   * Writes the DataFrame to the target path with overwrite mode,
   * excluding rows marked for deletion.
   *
   * @param dataFrame  The DataFrame to write.
   * @param targetPath The path of the target Delta table.
   */
  private def writeFull(dataFrame: DataFrame, targetPath: String): Unit = {

    logger.info(s"Writing full DataFrame to target path: $targetPath, excluding deletions.")

    val filteredDataFrame = if (dataFrame.columns.contains("OperationType")) {
      dataFrame.filter(lower(col("OperationType")) =!= "delete")
    } else {
      dataFrame
    }

    filteredDataFrame.write
      .format("delta")
      .mode("overwrite") // Use overwrite mode
      .save(targetPath) // Save to the target path
  }

  /*
   We will revisit this to make it metadata driven,
   The idea is to NOT PROCESS or SKIP processing the entities in the model
   which have been processed in last X Minutes, this will help with reruns.
   And re-processing data each time when there is re-try upon failure.
   For now the check has been included at a single point i.e. Writer Class.
   Ultimately it will make it into the config based framework.
    */
  def isDeltaTableRefreshedWithinXMinutes(adlsTargetDirectory: String, entity: String, reProcessingThresholdInMins: Int): Boolean = {
    val directoryPath = s"$adlsTargetDirectory/$entity"

    if (!DeltaTable.isDeltaTable(directoryPath)) return false

    try {
      val deltaTable = DeltaTable.forPath(directoryPath)
      val lastTimestamp = deltaTable.history(1).select("timestamp").collect()(0).getTimestamp(0)
      val currentTimestamp = java.sql.Timestamp.valueOf(java.time.LocalDateTime.now())
      val timeDifferenceMinutes = java.time.Duration.between(lastTimestamp.toLocalDateTime, currentTimestamp.toLocalDateTime).toMinutes
      timeDifferenceMinutes <= reProcessingThresholdInMins
    } catch {
      case _: Throwable => false
    }
  }
}
